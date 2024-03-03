using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public class ObstacleAvoidanceQuery
    {
        public const int DT_MAX_PATTERN_DIVS = 32;  ///< Max numver of adaptive divs.
        public const int DT_MAX_PATTERN_RINGS = 4;  ///< Max number of adaptive rings.

        public static bool SweepCircleCircle(Vector3 c0, float r0, Vector3 v, Vector3 c1, float r1, out float tmin, out float tmax)
        {
            tmin = 0;
            tmax = 0;

            float EPS = 0.0001f;
            Vector3 s = c1 - c0;
            float r = r0 + r1;
            float c = Vector2.Dot(s.XZ(), s.XZ()) - r * r;
            float a = Vector2.Dot(v.XZ(), v.XZ());
            if (a < EPS)
            {
                return false;  // not moving
            }

            // Overlap, calc time to exit.
            float b = Vector2.Dot(v.XZ(), s.XZ());
            float d = b * b - a * c;
            if (d < 0.0f)
            {
                return false; // no intersection.
            }

            a = 1.0f / a;
            float rd = (float)Math.Sqrt(d);
            tmin = (b - rd) * a;
            tmax = (b + rd) * a;

            return true;
        }
        public static bool IsectRaySeg(Vector3 ap, Vector3 u, Vector3 bp, Vector3 bq, out float t)
        {
            t = 0;

            Vector3 v = bq - bp;
            Vector3 w = ap - bp;
            float d = Vector2.Dot(u.XZ(), v.XZ());
            if (Math.Abs(d) < Utils.ZeroTolerance)
            {
                return false;
            }

            d = 1.0f / d;

            t = Vector2.Dot(v.XZ(), w.XZ()) * d;
            if (t < 0 || t > 1)
            {
                return false;
            }

            float s = Vector2.Dot(u.XZ(), w.XZ()) * d;
            if (s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Vector normalization that ignores the y-component.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 Normalize2D(Vector3 v)
        {
            Vector2 n = Vector2.Normalize(v.XZ());

            return new Vector3(n.X, v.Y, n.Y);
        }
        /// <summary>
        /// vector normalization that ignores the y-component.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="ang"></param>
        /// <returns></returns>
        public static Vector3 Rorate2D(Vector3 v, float ang)
        {
            float c = (float)Math.Cos(ang);
            float s = (float)Math.Sin(ang);

            Vector3 dest = Vector3.Zero;
            dest.X = v.X * c - v.Z * s;
            dest.Y = v.Y;
            dest.Z = v.X * s + v.Z * c;

            return dest;
        }

        private readonly int m_maxCircles;
        private readonly List<ObstacleCircle> m_circles = new();

        private readonly int m_maxSegments;
        private readonly List<ObstacleSegment> m_segments = new();

        private ObstacleAvoidanceParams m_params;
        private float m_invHorizTime;
        private float m_invVmax;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxCircles">Max circles</param>
        /// <param name="maxSegments">Max segments</param>
        public ObstacleAvoidanceQuery(int maxCircles, int maxSegments)
        {
            m_maxCircles = maxCircles;
            m_maxSegments = maxSegments;
        }

        public void AddCircle(Vector3 pos, float rad, Vector3 vel, Vector3 dvel)
        {
            if (m_circles.Count >= m_maxCircles)
            {
                return;
            }

            var cir = new ObstacleCircle
            {
                P = pos,
                Rad = rad,
                Vel = vel,
                DVel = dvel
            };

            m_circles.Add(cir);
        }
        public void AddSegment(Vector3 p, Vector3 q)
        {
            if (m_segments.Count >= m_maxSegments)
            {
                return;
            }

            var seg = new ObstacleSegment
            {
                P = p,
                Q = q
            };

            m_segments.Add(seg);
        }
        public IEnumerable<ObstacleCircle> GetObstacleCircles()
        {
            return m_circles.ToArray();
        }
        public IEnumerable<ObstacleSegment> GetObstacleSegments()
        {
            return m_segments.ToArray();
        }
        public void Reset()
        {
            m_circles.Clear();
            m_segments.Clear();
        }

        public int SampleVelocityGrid(ObstacleAvoidanceSampleRequest req, out Vector3 nvel)
        {
            Prepare(req.Pos, req.DVel);

            m_params = req.Param;
            m_invHorizTime = 1.0f / m_params.HorizTime;
            m_invVmax = req.VMax > 0 ? 1.0f / req.VMax : float.MaxValue;

            nvel = Vector3.Zero;

            req.Debug?.Reset();

            float cvx = req.DVel.X * m_params.VelBias;
            float cvz = req.DVel.Z * m_params.VelBias;
            float cs = req.VMax * 2 * (1 - m_params.VelBias) / (float)(m_params.GridSize - 1);
            float half = (m_params.GridSize - 1) * cs * 0.5f;

            float minPenalty = float.MaxValue;
            int ns = 0;

            for (int y = 0; y < m_params.GridSize; ++y)
            {
                for (int x = 0; x < m_params.GridSize; ++x)
                {
                    Vector3 vcand = Vector3.Zero;
                    vcand.X = cvx + x * cs - half;
                    vcand.Z = cvz + y * cs - half;

                    float vmaxCs = req.VMax + cs / 2;

                    if ((vcand.X * vcand.X) + (vcand.Z * vcand.Z) > (vmaxCs * vmaxCs))
                    {
                        continue;
                    }

                    var sReq = new ObstacleAvoidanceProcessSampleRequest
                    {
                        VCand = vcand,
                        Cs = cs,
                        Pos = req.Pos,
                        Rad = req.Rad,
                        Vel = req.Vel,
                        DVel = req.DVel,
                        MinPenalty = minPenalty,
                        Debug = req.Debug,
                    };

                    float penalty = ProcessSample(sReq);

                    ns++;
                    if (penalty < minPenalty)
                    {
                        minPenalty = penalty;
                        nvel = vcand;
                    }
                }
            }

            return ns;
        }
        public int SampleVelocityAdaptive(ObstacleAvoidanceSampleRequest req, out Vector3 nvel)
        {
            Prepare(req.Pos, req.DVel);

            m_params = req.Param;
            m_invHorizTime = 1.0f / m_params.HorizTime;
            m_invVmax = req.VMax > 0 ? 1.0f / req.VMax : float.MaxValue;

            req.Debug?.Reset();

            // Build sampling pattern aligned to desired velocity.
            Vector2[] pat = BuildSamplePattern(req.DVel, out int npat);

            // Start sampling.
            nvel = SamplePattern(req, pat, npat, out int ns);

            return ns;
        }
        private Vector2[] BuildSamplePattern(Vector3 dvel, out int npat)
        {
            // Build sampling pattern aligned to desired velocity.
            Vector2[] pat = new Vector2[DT_MAX_PATTERN_DIVS * DT_MAX_PATTERN_RINGS + 1];
            npat = 0;

            int ndivs = m_params.AdaptiveDivs;
            int nrings = m_params.AdaptiveRings;

            int nd = MathUtil.Clamp(ndivs, 1, DT_MAX_PATTERN_DIVS);
            int nr = MathUtil.Clamp(nrings, 1, DT_MAX_PATTERN_RINGS);
            float da = (1.0f / nd) * MathUtil.TwoPi;
            float ca = (float)Math.Cos(da);
            float sa = (float)Math.Sin(da);

            // desired direction
            Vector3 ddir1 = Normalize2D(dvel);
            Vector3 ddir2 = Rorate2D(ddir1, da * 0.5f); // rotated by da/2
            Vector3[] ddir = new Vector3[]
            {
                ddir1,
                ddir2
            };

            // Always add sample at zero
            pat[npat + 0] = Vector2.Zero;
            pat[npat + 1] = Vector2.Zero;
            npat++;

            for (int j = 0; j < nr; ++j)
            {
                float r = (float)(nr - j) / (float)nr;

                float vX = ddir[j % 2].X * r;
                float vZ = ddir[j % 2].Z * r;
                pat[npat] = new Vector2(vX, vZ);

                Vector2 last1 = pat[npat];
                Vector2 last2 = last1;
                npat++;

                for (int i = 1; i < nd - 1; i += 2)
                {
                    // get next point on the "right" (rotate CW)
                    pat[npat].X = last1.X * ca + last1.Y * sa;
                    pat[npat].Y = -last1.X * sa + last1.Y * ca;
                    // get next point on the "left" (rotate CCW)
                    pat[npat + 1].X = last2.X * ca - last2.Y * sa;
                    pat[npat + 1].Y = last2.X * sa + last2.Y * ca;

                    last1 = pat[npat];
                    last2 = pat[npat + 1];
                    npat += 2;
                }

                if ((nd & 1) == 0)
                {
                    pat[npat + 1].X = last2.X * ca - last2.Y * sa;
                    pat[npat + 1].Y = last2.X * sa + last2.Y * ca;
                    npat++;
                }
            }

            return pat;
        }
        private Vector3 SamplePattern(ObstacleAvoidanceSampleRequest req, Vector2[] pat, int npat, out int ns)
        {
            ns = 0;

            // Start sampling.
            float cr = req.VMax * (1.0f - m_params.VelBias);
            var res = new Vector3(req.DVel.X * m_params.VelBias, 0, req.DVel.Z * m_params.VelBias);
            int depth = m_params.AdaptiveDepth;

            for (int k = 0; k < depth; ++k)
            {
                float minPenalty = float.MaxValue;
                var bvel = Vector3.Zero;

                for (int i = 0; i < npat; ++i)
                {
                    var vcand = Vector3.Zero;
                    vcand.X = res.X + pat[i].X * cr;
                    vcand.Y = 0;
                    vcand.Z = res.Z + pat[i].Y * cr;

                    float vmaxD = req.VMax + 0.001f;

                    if ((vcand.X * vcand.X) + (vcand.Z * vcand.Z) > (vmaxD * vmaxD))
                    {
                        continue;
                    }

                    var sReq = new ObstacleAvoidanceProcessSampleRequest
                    {
                        VCand = vcand,
                        Cs = cr / 10,
                        Pos = req.Pos,
                        Rad = req.Rad,
                        Vel = req.Vel,
                        DVel = req.DVel,
                        MinPenalty = minPenalty,
                        Debug = req.Debug,
                    };

                    float penalty = ProcessSample(sReq);

                    ns++;
                    if (penalty < minPenalty)
                    {
                        minPenalty = penalty;
                        bvel = vcand;
                    }
                }

                res = bvel;

                cr *= 0.5f;
            }

            return res;
        }
        private void Prepare(Vector3 pos, Vector3 dvel)
        {
            // Prepare obstacles
            foreach (var cir in m_circles)
            {
                // Side
                Vector3 pa = pos;
                Vector3 pb = cir.P;

                Vector3 orig = Vector3.Zero;
                cir.Dp = pb - pa;
                cir.Dp.Normalize();
                Vector3 dv = cir.DVel - dvel;

                float a = Utils.TriArea2D(orig, cir.Dp, dv);
                if (a < 0.01f)
                {
                    var np = cir.Np;

                    np.X = -cir.Dp.Z;
                    np.Z = cir.Dp.X;

                    cir.Np = np;
                }
                else
                {
                    var np = cir.Np;

                    np.X = cir.Dp.Z;
                    np.Z = -cir.Dp.X;

                    cir.Np = np;
                }
            }

            foreach (var seg in m_segments)
            {
                // Precalc if the agent is really close to the segment.
                seg.Touch = Utils.DistancePtSegSqr2D(pos, seg.P, seg.Q, out _) < 0.0001f;
            }
        }
        /// <summary>
        /// Calculate the collision penalty for a given velocity vector
        /// </summary>
        /// <param name="req">Request</param>
        /// <returns>Returns the penalty</returns>
        private float ProcessSample(ObstacleAvoidanceProcessSampleRequest req)
        {
            // penalty for straying away from the desired and current velocities
            float vpen = m_params.WeightDesVel * (Utils.Distance2D(req.VCand, req.DVel) * m_invVmax);
            float vcpen = m_params.WeightCurVel * (Utils.Distance2D(req.VCand, req.Vel) * m_invVmax);

            // find the threshold hit time to bail out based on the early out penalty
            // (see how the penalty is calculated below to understnad)
            float minPen = req.MinPenalty - vpen - vcpen;
            float tThresold = (m_params.WeightToi / minPen - 0.1f) * m_params.HorizTime;
            if (tThresold - m_params.HorizTime > -float.Epsilon)
            {
                return req.MinPenalty; // already too much
            }

            // Find min time of impact and exit amongst all obstacles.
            float tmin = m_params.HorizTime;

            if (!ProcessSampleCircles(req, tThresold, ref tmin, out float side))
            {
                return req.MinPenalty;
            }

            if (!ProcessSampleSegment(req, tThresold, ref tmin))
            {
                return req.MinPenalty;
            }

            float spen = m_params.WeightSide * side;
            float tpen = m_params.WeightToi * (1.0f / (0.1f + tmin * m_invHorizTime));

            float penalty = vpen + vcpen + spen + tpen;

            // Store different penalties for debug viewing
            req.Debug?.AddSample(req.VCand, req.Cs, penalty, vpen, vcpen, spen, tpen);

            return penalty;
        }
        private bool ProcessSampleCircles(ObstacleAvoidanceProcessSampleRequest req, float tThresold, ref float tmin, out float side)
        {
            side = 0;

            int nside = 0;

            foreach (var cir in m_circles)
            {
                // RVO
                Vector3 vab = req.VCand * 2;
                vab -= req.Vel;
                vab -= cir.Vel;

                // Side
                side += MathUtil.Clamp(Math.Min(Vector2.Dot(cir.Dp.XZ(), vab.XZ()) * 0.5f + 0.5f, Vector2.Dot(cir.Np.XZ(), vab.XZ()) * 2), 0.0f, 1.0f);
                nside++;

                if (!SweepCircleCircle(req.Pos, req.Rad, vab, cir.P, cir.Rad, out float htmin, out float htmax))
                {
                    continue;
                }

                // Handle overlapping obstacles.
                if (htmin < 0.0f && htmax > 0.0f)
                {
                    // Avoid more when overlapped.
                    htmin = -htmin * 0.5f;
                }

                if (htmin >= 0.0f && htmin < tmin)
                {
                    // The closest obstacle is somewhere ahead of us, keep track of nearest obstacle.
                    tmin = htmin;
                    if (tmin < tThresold)
                    {
                        return false;
                    }
                }
            }

            // Normalize side bias, to prevent it dominating too much.
            if (nside > 0)
            {
                side /= nside;
            }

            return true;
        }
        private bool ProcessSampleSegment(ObstacleAvoidanceProcessSampleRequest req, float tThresold, ref float tmin)
        {
            foreach (var seg in m_segments)
            {
                float htmin;

                if (seg.Touch)
                {
                    // Special case when the agent is very close to the segment.
                    var sdir = seg.Q - seg.P;
                    var snorm = new Vector3(-sdir.Z, 0, sdir.X);
                    // If the velocity is pointing towards the segment, no collision.
                    if (Vector2.Dot(snorm.XZ(), req.VCand.XZ()) < 0.0f)
                    {
                        continue;
                    }
                    // Else immediate collision.
                    htmin = 0.0f;
                }
                else
                {
                    if (!IsectRaySeg(req.Pos, req.VCand, seg.P, seg.Q, out htmin))
                    {
                        continue;
                    }
                }

                // Avoid less when facing walls.
                htmin *= 2.0f;

                // The closest obstacle is somewhere ahead of us, keep track of nearest obstacle.
                if (htmin < tmin)
                {
                    tmin = htmin;
                    if (tmin < tThresold)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
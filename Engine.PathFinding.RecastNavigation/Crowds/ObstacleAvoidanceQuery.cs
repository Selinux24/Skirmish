using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Crowds
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
            if (Math.Abs(d) < 1e-6f)
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
            float d = (float)Math.Sqrt(v.X * v.X + v.Z * v.Z);
            if (d == 0)
            {
                return v;
            }

            d = 1.0f / d;

            Vector3 dest = Vector3.Zero;
            dest.X *= d;
            dest.Z *= d;

            return dest;
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
            dest.Z = v.X * s + v.Z * c;
            dest.Y = v.Y;

            return dest;
        }

        private ObstacleAvoidanceParams m_params;
        private float m_invHorizTime;
        private float m_vmax;
        private float m_invVmax;

        private int m_maxCircles;
        private ObstacleCircle[] m_circles;
        private int m_ncircles;

        private int m_maxSegments;
        private ObstacleSegment[] m_segments;
        private int m_nsegments;

        public ObstacleAvoidanceQuery()
        {

        }

        public bool Init(int maxCircles, int maxSegments)
        {
            m_maxCircles = maxCircles;
            m_ncircles = 0;
            m_circles = new ObstacleCircle[m_maxCircles];

            m_maxSegments = maxSegments;
            m_nsegments = 0;
            m_segments = new ObstacleSegment[m_maxSegments];

            return true;
        }
        public void Reset()
        {
            m_ncircles = 0;
            m_nsegments = 0;
        }
        public void AddCircle(Vector3 pos, float rad, Vector3 vel, Vector3 dvel)
        {
            if (m_ncircles >= m_maxCircles)
            {
                return;
            }

            if (m_circles[m_ncircles] == null)
            {
                m_circles[m_ncircles] = new ObstacleCircle();
            }

            ObstacleCircle cir = m_circles[m_ncircles];

            cir.P = pos;
            cir.Rad = rad;
            cir.Vel = vel;
            cir.DVel = dvel;

            m_ncircles++;
        }
        public void AddSegment(Vector3 p, Vector3 q)
        {
            if (m_nsegments >= m_maxSegments)
            {
                return;
            }

            if (m_segments[m_nsegments] == null)
            {
                m_segments[m_nsegments] = new ObstacleSegment();
            }

            ObstacleSegment seg = m_segments[m_nsegments];
            seg.P = p;
            seg.Q = q;

            m_nsegments++;
        }
        public int SampleVelocityGrid(
            Vector3 pos, float rad, float vmax,
            Vector3 vel, Vector3 dvel, out Vector3 nvel,
            ObstacleAvoidanceParams param,
            ObstacleAvoidanceDebugData debug = null)
        {
            Prepare(pos, dvel);

            m_params = param;
            m_invHorizTime = 1.0f / m_params.HorizTime;
            m_vmax = vmax;
            m_invVmax = vmax > 0 ? 1.0f / vmax : float.MaxValue;

            nvel = Vector3.Zero;

            if (debug != null)
            {
                debug.Reset();
            }

            float cvx = dvel.X * m_params.VelBias;
            float cvz = dvel.Z * m_params.VelBias;
            float cs = vmax * 2 * (1 - m_params.VelBias) / (float)(m_params.GridSize - 1);
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

                    float vmaxCs = vmax + cs / 2;

                    if ((vcand.X * vcand.X) + (vcand.Z * vcand.Z) > (vmaxCs * vmaxCs))
                    {
                        continue;
                    }

                    float penalty = ProcessSample(vcand, cs, pos, rad, vel, dvel, minPenalty, debug);

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
        public int SampleVelocityAdaptive(
            Vector3 pos, float rad, float vmax,
            Vector3 vel, Vector3 dvel, out Vector3 nvel,
            ObstacleAvoidanceParams param,
            ObstacleAvoidanceDebugData debug = null)
        {
            Prepare(pos, dvel);

            m_params = param;
            m_invHorizTime = 1.0f / m_params.HorizTime;
            m_vmax = vmax;
            m_invVmax = vmax > 0 ? 1.0f / vmax : float.MaxValue;

            if (debug != null)
            {
                debug.Reset();
            }

            // Build sampling pattern aligned to desired velocity.
            Vector2[] pat = new Vector2[DT_MAX_PATTERN_DIVS * DT_MAX_PATTERN_RINGS + 1];
            int npat = 0;

            int ndivs = m_params.AdaptiveDivs;
            int nrings = m_params.AdaptiveRings;
            int depth = m_params.AdaptiveDepth;

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

            // Start sampling.
            float cr = vmax * (1.0f - m_params.VelBias);
            Vector3 res = new Vector3(dvel.X * m_params.VelBias, 0, dvel[2] * m_params.VelBias);
            int ns = 0;

            for (int k = 0; k < depth; ++k)
            {
                float minPenalty = float.MaxValue;
                Vector3 bvel = Vector3.Zero;

                for (int i = 0; i < npat; ++i)
                {
                    Vector3 vcand = Vector3.Zero;
                    vcand.X = res.X + pat[i].X * cr;
                    vcand.Y = 0;
                    vcand.Z = res.Z + pat[i].Y * cr;

                    float vmaxD = vmax + 0.001f;

                    if ((vcand.X * vcand.X) + (vcand.Z * vcand.Z) > (vmaxD * vmaxD))
                    {
                        continue;
                    }

                    float penalty = ProcessSample(vcand, cr / 10, pos, rad, vel, dvel, minPenalty, debug);
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

            nvel = res;

            return ns;
        }
        public int GetObstacleCircleCount()
        {
            return m_ncircles;
        }
        public ObstacleCircle GetObstacleCircle(int i)
        {
            return m_circles[i];
        }
        public int GetObstacleSegmentCount()
        {
            return m_nsegments;
        }
        public ObstacleSegment GetObstacleSegment(int i)
        {
            return m_segments[i];
        }

        private void Prepare(Vector3 pos, Vector3 dvel)
        {
            // Prepare obstacles
            for (int i = 0; i < m_ncircles; ++i)
            {
                ObstacleCircle cir = m_circles[i];

                // Side
                Vector3 pa = pos;
                Vector3 pb = cir.P;

                Vector3 orig = Vector3.Zero;
                cir.Dp = pb - pa;
                cir.Dp.Normalize();
                Vector3 dv = cir.DVel - dvel;

                float a = Detour.TriArea2D(orig, cir.Dp, dv);
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

            for (int i = 0; i < m_nsegments; ++i)
            {
                ObstacleSegment seg = m_segments[i];

                // Precalc if the agent is really close to the segment.
                float r = 0.01f;
                seg.Touch = Detour.DistancePtSegSqr2D(pos, seg.P, seg.Q, out float t) < (r * r);
            }
        }
        /// <summary>
        /// Calculate the collision penalty for a given velocity vector
        /// </summary>
        /// <param name="vcand">sampled velocity</param>
        /// <param name="cs"></param>
        /// <param name="pos"></param>
        /// <param name="rad"></param>
        /// <param name="vel"></param>
        /// <param name="dvel">desired velocity</param>
        /// <param name="minPenalty">threshold penalty for early out</param>
        /// <param name="debug"></param>
        /// <returns></returns>
        private float ProcessSample(
            Vector3 vcand, float cs,
            Vector3 pos, float rad,
            Vector3 vel, Vector3 dvel,
            float minPenalty,
            ObstacleAvoidanceDebugData debug)
        {
            // penalty for straying away from the desired and current velocities
            float vpen = m_params.WeightDesVel * (Vector2.Distance(vcand.XZ(), dvel.XZ()) * m_invVmax);
            float vcpen = m_params.WeightCurVel * (Vector2.Distance(vcand.XZ(), vel.XZ()) * m_invVmax);

            // find the threshold hit time to bail out based on the early out penalty
            // (see how the penalty is calculated below to understnad)
            float minPen = minPenalty - vpen - vcpen;
            float tThresold = (m_params.WeightToi / minPen - 0.1f) * m_params.HorizTime;
            if (tThresold - m_params.HorizTime > -float.Epsilon)
            {
                return minPenalty; // already too much
            }

            // Find min time of impact and exit amongst all obstacles.
            float tmin = m_params.HorizTime;
            float side = 0;
            int nside = 0;

            for (int i = 0; i < m_ncircles; ++i)
            {
                ObstacleCircle cir = m_circles[i];

                // RVO
                Vector3 vab = vcand * 2;
                vab -= vel;
                vab -= cir.Vel;

                // Side
                side += MathUtil.Clamp(Math.Min(Vector2.Dot(cir.Dp.XZ(), vab.XZ()) * 0.5f + 0.5f, Vector2.Dot(cir.Np.XZ(), vab.XZ()) * 2), 0.0f, 1.0f);
                nside++;

                if (!SweepCircleCircle(pos, rad, vab, cir.P, cir.Rad, out float htmin, out float htmax))
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
                        return minPenalty;
                    }
                }
            }

            for (int i = 0; i < m_nsegments; ++i)
            {
                ObstacleSegment seg = m_segments[i];
                float htmin = 0;

                if (seg.Touch)
                {
                    // Special case when the agent is very close to the segment.
                    Vector3 sdir = seg.Q - seg.P;
                    Vector3 snorm = new Vector3(-sdir.Z, 0, sdir.X);
                    // If the velocity is pointing towards the segment, no collision.
                    if (Vector2.Dot(snorm.XZ(), vcand.XZ()) < 0.0f)
                    {
                        continue;
                    }
                    // Else immediate collision.
                    htmin = 0.0f;
                }
                else
                {
                    if (!IsectRaySeg(pos, vcand, seg.P, seg.Q, out htmin))
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
                        return minPenalty;
                    }
                }
            }

            // Normalize side bias, to prevent it dominating too much.
            if (nside > 0)
            {
                side /= nside;
            }

            float spen = m_params.WeightSide * side;
            float tpen = m_params.WeightToi * (1.0f / (0.1f + tmin * m_invHorizTime));

            float penalty = vpen + vcpen + spen + tpen;

            // Store different penalties for debug viewing
            if (debug != null)
            {
                debug.AddSample(vcand, cs, penalty, vpen, vcpen, spen, tpen);
            }

            return penalty;
        }
    }
}
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class SkyScattering
    {
        protected static float smEarthRadius = (6378.0f * 1000.0f);
        protected static float smAtmosphereRadius = 200000.0f;
        protected static float smViewerHeight = 1.0f;

        public TimeOfDay TimeOfDayController = null;

        protected List<Curve3D> mCurves = new List<Curve3D>();

        protected float mRayleighScattering;
        protected float mRayleighScattering4PI;
        protected float mSunSize;
        protected float mMieScattering;
        protected float mMieScattering4PI;

        protected float mSkyBrightness;
        protected float mMiePhaseAssymetry;

        protected float mOuterRadius;
        protected float mScale;
        protected Color mWavelength;
        protected float[] mWavelength4 = new float[3];
        protected float mRayleighScaleDepth;
        protected float mMieScaleDepth;

        protected float mSphereInnerRadius;
        protected float mSphereOuterRadius;

        protected float mExposure;
        protected float mNightInterpolant;
        protected float mZOffset;

        protected Vector3 mLightDir;
        protected Vector3 mSunDir;

        protected float mSunAzimuth;
        protected float mSunElevation;
        protected float mMoonAzimuth;
        protected float mMoonElevation;

        protected float mTimeOfDay;

        protected float mBrightness;

        protected Color mNightColor;
        protected Color mNightFogColor;

        protected Color mAmbientColor;   ///< Not a field
        protected Color mSunColor;       ///< Not a field
        protected Color mFogColor;       ///< Not a field

        protected Color mAmbientScale;
        protected Color mSunScale;
        protected Color mFogScale;

        protected SceneLightDirectional mLight;

        protected bool mCastShadows;
        protected int mStaticRefreshFreq;
        protected int mDynamicRefreshFreq;
        protected bool mDirty;

        protected List<Vector3> mSkyPoints;

        public SkyScattering()
        {
            this.TimeOfDayController = new TimeOfDay();

            // Rayleigh scattering constant.
            this.mRayleighScattering = 0.0035f;
            this.mRayleighScattering4PI = this.mRayleighScattering * 4.0f * MathUtil.Pi;

            // Mie scattering constant.
            this.mMieScattering = 0.0045f;
            this.mMieScattering4PI = this.mMieScattering * 4.0f * MathUtil.Pi;

            // Overall scatter scalar.
            this.mSkyBrightness = 25.0f;

            // The Mie phase asymmetry factor.
            this.mMiePhaseAssymetry = -0.75f;

            this.mSphereInnerRadius = 1.0f;
            this.mSphereOuterRadius = 1.0f * 1.025f;
            this.mScale = 1.0f / (this.mSphereOuterRadius - this.mSphereInnerRadius);

            // 650 nm for red
            // 570 nm for green
            // 475 nm for blue
            this.mWavelength = new Color(0.650f, 0.570f, 0.475f, 0);

            this.mWavelength4[0] = (float)Math.Pow(this.mWavelength[0], 4.0f);
            this.mWavelength4[1] = (float)Math.Pow(this.mWavelength[1], 4.0f);
            this.mWavelength4[2] = (float)Math.Pow(this.mWavelength[2], 4.0f);

            this.mRayleighScaleDepth = 0.25f;
            this.mMieScaleDepth = 0.1f;

            this.mAmbientColor = new Color(0, 0, 0, 1.0f);
            this.mAmbientScale = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            this.mSunColor = new Color(0, 0, 0, 1.0f);
            this.mSunScale = Color.White;

            this.mFogColor = new Color(0, 0, 0, 1.0f);
            this.mFogScale = Color.White;

            this.mExposure = 1.0f;
            this.mNightInterpolant = 0;
            this.mZOffset = 0.0f;

            this.mTimeOfDay = 0;

            this.mSunAzimuth = 0.0f;
            this.mSunElevation = 35.0f;

            this.mMoonAzimuth = 0.0f;
            this.mMoonElevation = 45.0f;

            this.mBrightness = 1.0f;

            this.mCastShadows = true;
            this.mStaticRefreshFreq = 8;
            this.mDynamicRefreshFreq = 8;
            this.mDirty = true;

            this.mNightColor = new Color(0.0196078f, 0.0117647f, 0.109804f, 1.0f);
            this.mNightFogColor = this.mNightColor;
            this.mSunSize = 1.0f;

            this._generateSkyPoints();
        }

        protected float _getRayleighPhase(float fCos2)
        {
            return 0.75f + 0.75f * fCos2;
        }
        protected float _getMiePhase(float fCos, float fCos2, float g, float g2)
        {
            return 1.5f * ((1.0f - g2) / (2.0f + g2)) * (1.0f + fCos2) / (float)Math.Pow(Math.Abs(1.0f + g2 - 2.0f * g * fCos), 1.5f);
        }
        protected float _vernierScale(float fCos)
        {
            float x = 1.0f - fCos;
            return 0.25f * (float)Math.Exp(-0.00287f + x * (0.459f + x * (3.83f + x * ((-6.80f + (x * 5.25f))))));
        }

        protected void _generateSkyPoints()
        {
            uint rings = 60, segments = 20;

            Vector3 tmpPoint = new Vector3(0, 0, 0);

            // Establish constants used in sphere generation.
            float deltaRingAngle = (MathUtil.Pi / (float)(rings * 2));
            float deltaSegAngle = (MathUtil.TwoPi / (float)segments);

            // Generate the group of rings for the sphere.
            for (uint ring = 0; ring < 2; ring++)
            {
                float r0 = (float)Math.Sin(ring * deltaRingAngle);
                float y0 = (float)Math.Cos(ring * deltaRingAngle);

                // Generate the group of segments for the current ring.
                for (int seg = 0; seg < segments + 1; seg++)
                {
                    float x0 = r0 * (float)Math.Sin(seg * deltaSegAngle);
                    float z0 = r0 * (float)Math.Cos(seg * deltaSegAngle);

                    tmpPoint = new Vector3(x0, z0, y0);
                    tmpPoint.Normalize();

                    tmpPoint.X *= smEarthRadius + smAtmosphereRadius;
                    tmpPoint.Y *= smEarthRadius + smAtmosphereRadius;
                    tmpPoint.Z *= smEarthRadius + smAtmosphereRadius;
                    tmpPoint.Z -= smEarthRadius;

                    if (ring == 1)
                    {
                        this.mSkyPoints.Add(tmpPoint);
                    }
                }
            }
        }

        protected void _getColor(Vector3 pos, out Color outColor)
        {
            outColor = Color.Zero;

            /*
F32 scaleOverScaleDepth = mScale / mRayleighScaleDepth;
   F32 rayleighBrightness = mRayleighScattering * mSkyBrightness;
   F32 mieBrightness = mMieScattering * mSkyBrightness;

   Point3F invWaveLength(  1.0f / mWavelength4[0],
                           1.0f / mWavelength4[1],
                           1.0f / mWavelength4[2] );

   Point3F v3Pos = pos / 6378000.0f;
   v3Pos.z += mSphereInnerRadius;

   Point3F newCamPos( 0, 0, smViewerHeight );

   VectorF v3Ray = v3Pos - newCamPos;
   F32 fFar = v3Ray.len();
   v3Ray / fFar;
   v3Ray.normalizeSafe();

   Point3F v3Start = newCamPos;
   F32 fDepth = mExp( scaleOverScaleDepth * (mSphereInnerRadius - smViewerHeight ) );
   F32 fStartAngle = mDot( v3Ray, v3Start );

   F32 fStartOffset = fDepth * _vernierScale( fStartAngle );

   F32 fSampleLength = fFar / 2.0f;
   F32 fScaledLength = fSampleLength * mScale;
   VectorF v3SampleRay = v3Ray * fSampleLength;
   Point3F v3SamplePoint = v3Start + v3SampleRay * 0.5f;

   Point3F v3FrontColor( 0, 0, 0 );
   for ( U32 i = 0; i < 2; i++ )
   {
      F32 fHeight = v3SamplePoint.len();
      F32 fDepth = mExp( scaleOverScaleDepth * (mSphereInnerRadius - smViewerHeight) );
      F32 fLightAngle = mDot( mLightDir, v3SamplePoint ) / fHeight;
      F32 fCameraAngle = mDot( v3Ray, v3SamplePoint ) / fHeight;

      F32 fScatter = (fStartOffset + fDepth * ( _vernierScale( fLightAngle ) - _vernierScale( fCameraAngle ) ));
      Point3F v3Attenuate( 0, 0, 0 );

      F32 tmp = mExp( -fScatter * (invWaveLength[0] * mRayleighScattering4PI + mMieScattering4PI) );
      v3Attenuate.x = tmp;

      tmp = mExp( -fScatter * (invWaveLength[1] * mRayleighScattering4PI + mMieScattering4PI) );
      v3Attenuate.y = tmp;

      tmp = mExp( -fScatter * (invWaveLength[2] * mRayleighScattering4PI + mMieScattering4PI) );
      v3Attenuate.z = tmp;

      v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
      v3SamplePoint += v3SampleRay;
   }

   Point3F mieColor = v3FrontColor * mieBrightness;
   Point3F rayleighColor = v3FrontColor * (invWaveLength * rayleighBrightness);
   Point3F v3Direction = newCamPos - v3Pos;
   v3Direction.normalize();

   F32 fCos = mDot( mLightDir, v3Direction ) / v3Direction.len();
   F32 fCos2 = fCos * fCos;

   F32 g = -0.991f;
   F32 g2 = g * g;
   F32 miePhase = _getMiePhase( fCos, fCos2, g, g2 );

   Point3F color = rayleighColor + (miePhase * mieColor);
   ColorF tmp( color.x, color.y, color.z, color.y );

   Point3F expColor( 0, 0, 0 );
   expColor.x = 1.0f - exp(-mExposure * color.x);
   expColor.y = 1.0f - exp(-mExposure * color.y);
   expColor.z = 1.0f - exp(-mExposure * color.z);

   tmp.set( expColor.x, expColor.y, expColor.z, 1.0f );

   if ( !tmp.isValidColor() )
   {
      F32 len = expColor.len();
      if ( len > 0 )
         expColor /= len;
   }

   outColor->set( expColor.x, expColor.y, expColor.z, 1.0f );             
             */
        }
        protected void _getFogColor(out Color outColor)
        {
            Vector3 scatterPos = new Vector3(0, 0, 0);

            float sunBrightness = mSkyBrightness;
            mSkyBrightness *= 0.25f;

            float yaw = 0, pitch = 0, originalYaw = 0;
            Vector3 fwd = TimeOfDay.CalcLightDirection(pitch, yaw);
            originalYaw = yaw;
            pitch = MathUtil.DegreesToRadians(10.0f);

            Color tmpColor = new Color(0, 0, 0);

            outColor = Color.Zero;

            int i = 0;
            for (i = 0; i < 10; i++)
            {
                scatterPos = TimeOfDay.CalcLightDirection(pitch, yaw);

                scatterPos.X *= smEarthRadius + smAtmosphereRadius;
                scatterPos.Y *= smEarthRadius + smAtmosphereRadius;
                scatterPos.Z *= smEarthRadius + smAtmosphereRadius;
                scatterPos.Y -= smEarthRadius;

                this._getColor(scatterPos, out tmpColor);
                outColor += tmpColor;

                if (i <= 5)
                    yaw += MathUtil.DegreesToRadians(5.0f);
                else
                {
                    originalYaw += MathUtil.DegreesToRadians(-5.0f);
                    yaw = originalYaw;
                }

                yaw = MathUtil.Mod(yaw, MathUtil.TwoPi);
            }

            if (i > 0)
            {
                outColor *= 1f / (float)i;
            }

            mSkyBrightness = sunBrightness;
        }
        protected void _getAmbientColor(out Color outColor)
        {
            Color tmpColor = new Color(0, 0, 0, 0);
            float count = 0;

            // Disable mieScattering for purposes of calculating the ambient color.
            float oldMieScattering = mMieScattering;
            mMieScattering = 0.0f;

            outColor = Color.Zero;

            for (int i = 0; i < mSkyPoints.Count; i++)
            {
                Vector3 pnt = mSkyPoints[i];

                this._getColor(pnt, out tmpColor);
                outColor += tmpColor;
                count++;
            }

            if (count > 0)
                outColor *= 1f / count;

            mMieScattering = oldMieScattering;
        }
        protected void _getSunColor(out Color outColor)
        {
            uint count = 0;
            Color tmpColor = new Color(0, 0, 0);
            Vector3 tmpVec = new Vector3(0, 0, 0);

            tmpVec = mLightDir;
            tmpVec.X *= smEarthRadius + smAtmosphereRadius;
            tmpVec.Y *= smEarthRadius + smAtmosphereRadius;
            tmpVec.Z *= smEarthRadius + smAtmosphereRadius;
            tmpVec.Z -= smAtmosphereRadius;

            outColor = Color.Zero;

            for (uint i = 0; i < 10; i++)
            {
                this._getColor(tmpVec, out tmpColor);
                outColor += tmpColor;
                tmpVec.X += (smEarthRadius * 0.5f) + (smAtmosphereRadius * 0.5f);
                count++;
            }

            if (count > 0)
            {
                outColor *= 1f / (float)count;
            }
        }
        protected void _interpolateColors()
        {
            mFogColor = new Color(0, 0, 0, 0);
            mAmbientColor = new Color(0, 0, 0, 0);
            mSunColor = new Color(0, 0, 0, 0);

            _getFogColor(out mFogColor);
            _getAmbientColor(out mAmbientColor);
            _getSunColor(out mSunColor);

            mAmbientColor *= mAmbientScale;
            mSunColor *= mSunScale;
            mFogColor *= mFogScale;

            mMieScattering = (mCurves[1].GetPosition(mTimeOfDay).X * mSunSize); //Scale the size of the sun's disk

            //Color moonTemp = mMoonTint;
            Color nightTemp = mNightColor;

            //moonTemp.interpolate(mNightColor, mMoonTint, mCurves[4].getVal(mTimeOfDay));
            //nightTemp.interpolate(mMoonTint, mNightColor, mCurves[4].getVal(mTimeOfDay));

            //mFogColor.interpolate(mFogColor, mNightFogColor, mCurves[3].getVal(mTimeOfDay));//mNightInterpolant );
            //mFogColor.alpha = 1.0f;

            //mAmbientColor.interpolate(mAmbientColor, mNightColor, mCurves[3].getVal(mTimeOfDay));//mNightInterpolant );
            //mSunColor.interpolate(mSunColor, mMoonTint, mCurves[3].getVal(mTimeOfDay));//mNightInterpolant );
        }

        protected void _initCurves()
        {

        }

        protected void _conformLights()
        {
            _initCurves();

            float val = mCurves[0].GetPosition(this.mTimeOfDay).X;
            this.mNightInterpolant = 1.0f - val;

            Vector3 lightDirection;
            float brightness;

            // Build the light direction from the azimuth and elevation.
            float elevation = MathUtil.DegreesToRadians(MathUtil.Clamp(this.mSunElevation, -360, +360));
            float azimuth = MathUtil.DegreesToRadians(MathUtil.Clamp(this.mSunAzimuth, 0, 359));
            lightDirection = TimeOfDay.CalcLightDirection(elevation, azimuth);
            this.mSunDir = -lightDirection;

            //yaw = MathUtil.DegreesToRadians(MathUtil.Clamp(this.mMoonAzimuth, 0, 359));
            //pitch = MathUtil.DegreesToRadians(MathUtil.Clamp(this.mMoonElevation, -360, +360));
            //getVectorFromAngles(this.mMoonLightDir, yaw, pitch);
            //this.mMoonLightDir.normalize();
            //this.mMoonLightDir = -this.mMoonLightDir;

            brightness = mCurves[2].GetPosition(this.mTimeOfDay).X;

            if (mNightInterpolant >= 1.0f)
            {
                //lightDirection = -this.mMoonLightDir;
            }

            this.mLight.Direction = -lightDirection;
            //this.mLight.Brightness(brightness * this.mBrightness);
            this.mLightDir = lightDirection;

            // Have to do interpolation
            // after the light direction is set
            // otherwise the sun color will be invalid.
            this._interpolateColors();

            //this.mLight.Ambient(mAmbientColor);
            this.mLight.DiffuseColor = this.mSunColor;
            this.mLight.SpecularColor = this.mSunColor;

            //fog.color = mFogColor;
        }

        protected void Update(float elapsedSeconds)
        {
            this.TimeOfDayController.Update(elapsedSeconds);
        }
    }
}

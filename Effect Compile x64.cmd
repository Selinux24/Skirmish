cd Engine\Resources
del *.fxo
del *.cod
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderNull.fxo ShaderNull.fx

"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderDefaultSprite.fxo ShaderDefaultSprite.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderDefaultFont.fxo ShaderDefaultFont.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderDefaultCubemap.fxo ShaderDefaultCubemap.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderDefaultBillboard.fxo ShaderDefaultBillboard.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderDefaultCPUParticles.fxo ShaderDefaultCPUParticles.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderDefaultGPUParticles.fxo ShaderDefaultGPUParticles.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderDefaultBasic.fxo ShaderDefaultBasic.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderDefaultTerrain.fxo ShaderDefaultTerrain.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderDefaultSkyScattering.fxo ShaderDefaultSkyScattering.fx

"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderDeferredBasic.fxo ShaderDeferredBasic.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderDeferredTerrain.fxo ShaderDeferredTerrain.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderDeferredComposer.fxo ShaderDeferredComposer.fx

"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderShadowBillboard.fxo ShaderShadowBillboard.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderShadowBasic.fxo ShaderShadowBasic.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderShadowTerrain.fxo ShaderShadowTerrain.fx

"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderPostBlur.fxo ShaderPostBlur.fx

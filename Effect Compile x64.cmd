cd Engine\Resources
del *.fxo
del *.cod
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderNull.fxo ShaderNull.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderFont.fxo ShaderFont.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderSprite.fxo ShaderSprite.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderCubemap.fxo ShaderCubemap.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderBillboard.fxo ShaderBillboard.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderCPUParticles.fxo ShaderCPUParticles.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderGPUParticles.fxo ShaderGPUParticles.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderTerrain.fxo ShaderTerrain.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderBasic.fxo ShaderBasic.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderShadow.fxo ShaderShadow.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderGBuffer.fxo ShaderGBuffer.fx
"%DXSDK_DIR%bin\x64\"fxc /O0 /Fc /Zi /T  fx_5_0 /Fo ShaderDeferred.fxo ShaderDeferred.fx

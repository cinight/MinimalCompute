# Minimal Compute Shader Examples
Minimal test scenes contains compute shaders, compute buffers etc
Playing with the transport between CPU <-> GPU

Unity version : 2019.3+, but should work in older versions as well

Tested with : Win DX11, Mac Metal

| Scene | Image | Description |
| --- | - | --- |
| `ComputeUAVTexture` | ![](READMEImages/ComputeUAVTexture.gif) | The most basic one, edit texture with compute shader |
| `StructuredBufferWithCompute` | ![](READMEImages/StructuredBufferWithCompute.JPG) | Another basic one, use compute to calculate some data and send back to CPU |
| `StructuredBufferNoCompute` | ![](READMEImages/StructuredBufferNoCompute.gif) | ComputeBuffer doesn't always need to stick with ComputeShader |
| `IndirectCompute` | ![](READMEImages/IndirectCompute.gif) | Simple indirect compute (indirect dispatch) and CopyCount |
| `ComputeParticlesDirect` | ![](READMEImages/ComputeParticlesDirect.gif) | GPU Particle, drawing fixed no. of particles |
| `ComputeParticlesIndirect` | ![](READMEImages/ComputeParticlesIndirect.gif) | GPU Particle, drawing dynamic no. of particles, no need to read back to CPU! |
| `ComputeVertex` | ![](READMEImages/ComputeVertex.gif) | Replace vertex buffer with StructuredBuffer and drive vertex displacement by compute |
| `UAVInShader` | ![](READMEImages/UAVInShader.gif) | Read some data back to CPU from fragment shader |
| `AsyncGPUReadback` | ![](READMEImages/AsyncGPUReadback.gif) | Similar to StructuredBufferWithCompute, but use AsyncGPUReadback to get array data back to CPU |
| `AsyncGPUReadbackTex` | ![](READMEImages/AsyncGPUReadbackTex.gif) | Similar to ComputeUAVTexture, but use AsyncGPUReadback to get texture data back to CPU |

-------------

Disclaimer: The stuff here might not be the best practice / optimized :'(. But at least they works. Play them for fun.

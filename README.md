# Minimal Compute Shader Examples
Minimal test scenes contains compute shaders, compute buffers etc
Playing with the transport between CPU <-> GPU

Unity version : 2022.2.4f1+, contains both `BuiltinRP` and `Universal RP (URP)` scenes \
See branches for older Unity versions \
Tested with : Win DX11

| Scene | Image | Description |
| --- | - | --- |
| `ComputeUAVTexture` | ![](READMEimages/ComputeUAVTexture.gif) | The most basic one, edit texture with compute shader |
| `AsyncGPUReadbackTex` | ![](READMEimages/AsyncGPUReadbackTex.gif) | Similar to `ComputeUAVTexture`, but use AsyncGPUReadback to get texture data back to CPU |
| `ComputeUAVTexFlow` | ![](READMEimages/ComputeUAVTexFlow.gif) | Example of using compute to animate texture pixels |
| `ComputePaintTexture` | ![](READMEimages/ComputePaintTexture.gif) | Paint the texture by sending object positions to compute shader |
| `ComputePaintTexture_DFT` | ![](READMEimages/ComputePaintTexture_DFT.gif) | Similar to above but drawing with Epicycles using Discrete Fourier Transform. Ref to [The Coding Train's youtube video](https://www.youtube.com/watch?v=MY4luNgGfms) |
| `Fluid` | ![](READMEimages/Fluid.gif) | GPU Fluid. Ref to [Scrawk/GPU-GEMS-2D-Fluid-Simulation](https://github.com/Scrawk/GPU-GEMS-2D-Fluid-Simulation) |
| `StructuredBufferWithCompute` | ![](READMEimages/StructuredBufferWithCompute.gif) | Another basic one, use compute to calculate some data and send back to CPU |
| `AsyncGPUReadback` | ![](READMEimages/AsyncGPUReadback.gif) | Similar to `StructuredBufferWithCompute`, but use AsyncGPUReadback to get array data back to CPU |
| `StructuredBufferNoCompute` | ![](READMEimages/StructuredBufferNoCompute.gif) | ComputeBuffer doesn't always need to stick with ComputeShader |
| `IndirectCompute` | ![](READMEimages/IndirectCompute.gif) | Simple indirect compute (indirect dispatch) and CopyCount |
| `IndirectReflectedStar` | ![](READMEimages/IndirectReflectedStar.gif) | Draw stars on the screen only if the pixels are bright enough |
| `ComputeSketch` | ![](READMEimages/ComputeSketch.JPG) | Draw quads on the screen with color filled by compute shader |
| `ComputeParticlesDirect` | ![](READMEimages/ComputeParticlesDirect.gif) | GPU Particle, drawing fixed no. of particles |
| `ComputeParticlesIndirect` | ![](READMEimages/ComputeParticlesIndirect.gif) | GPU Particle, drawing dynamic no. of particles, no need to read back to CPU! |
| `ComputeVertex` | ![](READMEimages/ComputeVertex.gif) | Replace vertex buffer with StructuredBuffer and drive vertex displacement by compute |
| `SkinnedMeshBuffer` | ![](READMEimages/SkinnedMeshBuffer.gif) | Blend the vertex data from 2 SkinnedMeshRenderer vertex buffer and render it with MeshRenderer |
| `SkinnedMeshBuffer_DiffMesh` | ![](READMEimages/SkinnedMeshBuffer_DiffMesh.gif) | Similar to above but blending 2 different SkinnedMeshes. The blended triangles are drawn with DrawMeshInstancedIndirect() |
| `ComputeVertexLit` | ![](READMEimages/ComputeVertexLit.gif) | A usecase of ComputeVertex so that different shader passes share same vertex data |
| `UAVInShader` | ![](READMEimages/UAVInShader.gif) | Read some data back to CPU from fragment shader |
| `AsyncGPUReadbackMesh` | ![](READMEimages/AsyncGPUReadbackMesh.gif) | It is much faster to update mesh vertices with compute + AsyncGPUReadback to get the vertex data back to CPU for physics |

-------------

Disclaimer: The stuff here might not be the best practice / optimized :'(. But at least they works. Play them for fun.

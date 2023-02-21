Traffic (06_1 & 06_2): CPU instructs -> GPU writes the data -> Shader uses this data instead of original vertex buffer from MehsRenderer
Traffic (other): CPU instructs -> shader is able to read the skinned vertex buffer from other meshes, do something with it and use it instead of it's original vertex buffer from MehsRenderer

You can treat these vertex buffers as structured buffers.
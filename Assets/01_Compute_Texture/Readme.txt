Traffic: CPU instructs -> GPU write into texture -> Shader reads the texture for rendering

Compute shaders write to the texture pixels on GPU side and shader can use the texture directly for rendering.

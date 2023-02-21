Traffic: CPU instructs -> Shader does rendering also writes the data -> CPU reads the data for doing other stuff

Not only compute shaders let CPU to read it's data, normal shader can also do that.
If you use SRP, you can use the ShaderDebugPrint feature. I have a sample here: https://github.com/cinight/CustomSRP/tree/master/Assets/SRP0406_ShaderDebugPrint


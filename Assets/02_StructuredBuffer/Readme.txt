Traffic (02_1): CPU send data -> shader takes the data for rendering
Traffic (other): CPU instructs -> GPU writes the data -> shader takes the data for rendering

Instead of textures (which you can see it as an array of pixels values), it can be structured buffers (an array of custom set of values).
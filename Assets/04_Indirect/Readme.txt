Traffic: CPU instructs (tell what maximum workload is) -> GPU filters the work based on some conditions -> GPU perform only on the filtered work -> Shader takes the data for rendering

This way the data never need to go back to CPU, and GPU doesn't always need to do expensive maths on all of the data, so it's fast.
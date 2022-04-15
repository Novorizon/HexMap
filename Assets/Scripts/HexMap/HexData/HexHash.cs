using UnityEngine;

public struct HexHash
{

    public float a, b, c, d, e;
    public float[] features;
    //public float[] subfeatures;
    public static HexHash Create()
    {
        HexHash hash;
        hash.a = Random.value * 0.999f;
        hash.b = Random.value * 0.999f;
        hash.c = Random.value * 0.999f;
        hash.d = Random.value * 0.999f;
        hash.e = Random.value * 0.999f;
        hash.features = new float[HexMetrics.FeatureCount];
        for (int i = 0; i < HexMetrics.FeatureCount; i++)
        {
            hash.features[i] = Random.value * 0.999f;
        }
        //hash.subfeatures = new float[3];
        //for (int i = 0; i < 3; i++)
        //{
        //    hash.subfeatures[i] = Random.value * 0.999f;
        //}
        return hash;
    }
}
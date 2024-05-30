using System;
using UnityEngine;

[Serializable]
public struct SerializableVector3
{
    public float[] coords;
    public SerializableVector3(float rX, float rY, float rZ)
    {
        coords = new float[3] { rX, rY, rZ };
    }

    public static implicit operator Vector3(SerializableVector3 rValue)
    {
        return new Vector3(rValue.coords[0], rValue.coords[1], rValue.coords[2]);
    }

    public static implicit operator SerializableVector3(Vector3 rValue)
    {
        return new SerializableVector3(rValue.x, rValue.y, rValue.z);
    }

    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}]", coords[0], coords[1], coords[2]);
    }
}

[Serializable]
public struct SerializableQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;

    public SerializableQuaternion(float rX, float rY, float rZ, float rW)
    {
        x = rX;
        y = rY;
        z = rZ;
        w = rW;
    }

    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
    }


    public static implicit operator Quaternion(SerializableQuaternion rValue)
    {
        return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
    }

    public static implicit operator SerializableQuaternion(Quaternion rValue)
    {
        return new SerializableQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
    }
}




[Serializable]
public struct SerializableColor
{
    public float[] channels;

    public SerializableColor(float r, float g, float b, float a)
    {
        channels = new float[4] { r, g, b, a };
    }

    public static implicit operator Color(SerializableColor rValue)
    {
        return new Color(rValue.channels[0], rValue.channels[1], rValue.channels[2], rValue.channels[3]);
    }

    public static implicit operator SerializableColor(Color rValue)
    {
        return new SerializableColor(rValue.r, rValue.g, rValue.b, rValue.a);
    }
}
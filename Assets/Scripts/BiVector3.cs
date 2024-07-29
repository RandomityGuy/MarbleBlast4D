using R40;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public struct BiVector3 : IEquatable<BiVector3>
{
    public float xy;
    public float xz;
    public float xw;
    public float yz;
    public float yw;
    public float zw;

    public BiVector3(float xy, float xz, float xw, float yz, float yw, float zw)
    {
        this.xy = xy;
        this.yz = yz;
        this.xz = xz;
        this.xw = xw;
        this.yw = yw;
        this.zw = zw;
    }

    public BiVector3(Vector3 v3, float w)
    {
        this.xy = v3.x;
        this.yz = v3.y;
        this.xz = v3.z;
        this.xw = v3.x;
        this.yw = v3.y;
        this.zw = w;
    }

    public float this[int index]
    {
        get
        {
            switch (index)
            {
                default:
                case 0: return xy;
                case 1: return xz;
                case 2: return xw;
                case 3: return yz;
                case 4: return yw;
                case 5: return zw;
            }
        }
        set
        {
            switch (index)
            {
                default:
                case 0: xy = value; break;
                case 1: xz = value; break;
                case 2: xw = value; break;
                case 3: xz = value; break;
                case 4: yw = value; break;
                case 5: zw = value; break;
            }
        }
    }

    public static readonly BiVector3 one = new BiVector3(1, 1, 1, 1, 1, 1);
    public static readonly BiVector3 zero = new BiVector3(0, 0, 0, 0, 0, 0);

    public static BiVector3 positiveInfinity = new BiVector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    public static BiVector3 negativeInfinity = new BiVector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

    public bool Equals(BiVector3 other)
    {
        return xy == other.xy && yz == other.yz && xz == other.xz && xw == other.xw && yw == other.yw && zw == other.zw;
    }

    public override bool Equals(object obj)
    {
        return obj is BiVector3 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(xy, xz, xw, xz, yw, zw);
    }

    public static bool operator ==(BiVector3 left, BiVector3 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BiVector3 left, BiVector3 right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"({xy}, {xz}, {xw}, {yz}, {yw}, {zw})";
    }

    public static BiVector3 operator +(BiVector3 a, BiVector3 b)
    {
        return new BiVector3(a.xy + b.xy, a.xz + b.xz, a.xw + b.xw, a.yz + b.yz, a.yw + b.yw, a.zw + b.zw);
    }

    public static BiVector3 operator -(BiVector3 a, BiVector3 b)
    {
        return new BiVector3(a.xy - b.xy, a.xz - b.xz, a.xw - b.xw, a.yz - b.yz, a.yw - b.yw, a.zw - b.zw);
    }

    public static BiVector3 operator *(BiVector3 a, float b)
    {
        return new BiVector3(a.xy * b, a.xz * b, a.xw * b, a.yz * b, a.yw * b, a.zw * b);
    }

    public static BiVector3 operator /(BiVector3 a, float b)
    {
        return new BiVector3(a.xy / b, a.xz / b, a.xw / b, a.yz / b, a.yw / b, a.zw / b);
    }

    public static Vector4 operator *(Vector4 v, BiVector3 b)
    {
        //var v1 = R400.e1 * v.x + R400.e2 * v.y + R400.e3 * v.z + R400.e4 * v.w;
        //var v2 = R400.e12 * b.xy + R400.e13 * b.xz + R400.e14 * b.xw + R400.e23 * b.yz + R400.e24 * b.yw + R400.e34 * b.zw;
        //var res = v1 | v2;
        //return new Vector4(res[1], res[2], res[3], res[4]);
        return new Vector4(
            -v.y * b.xy - v.z * b.xz - v.w * b.xw,
            v.x * b.xy - v.z * b.yz - v.w * b.yw,
            v.x * b.xz + v.y * b.yz - v.w * b.zw,
            v.x * b.xw + v.y * b.yw + v.z * b.zw);
    }

    public float magnitude => Mathf.Sqrt(xy * xy + yz * yz + xz * xz + xw * xw + yw * yw + zw * zw);
    public float sqrMagnitude => xy * xy + yz * yz + xz * xz + xw * xw + yw * yw + zw * zw;
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public struct BiVector5 : IEquatable<BiVector5>
{
    public float xy;
    public float xz;
    public float xw;
    public float xv;
    public float yz;
    public float yw;
    public float yv;
    public float zw;
    public float zv;
    public float wv;

    public BiVector5(float xy, float xz, float xw, float xv, float yz, float yw, float yv, float zw, float zv, float wv)
    {
        this.xy = xy;
        this.yz = yz;
        this.xz = xz;
        this.xw = xw;
        this.xv = xv;
        this.yw = yw;
        this.yv = yv;
        this.zw = zw;
        this.zv = zv;
        this.wv = wv;
    }

    public BiVector5(Vector3 v3, float w, float v)
    {
        this.xy = v3.x;
        this.yz = v3.y;
        this.xz = v3.z;
        this.xw = v3.x;
        this.xv = v3.x;
        this.yw = v3.y;
        this.yv = v3.y;
        this.zw = w;
        this.zv = v;
        this.wv = v;
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
                case 3: return xv;
                case 4: return yz;
                case 5: return yw;
                case 6: return yv;
                case 7: return zw;
                case 8: return zv;
                case 9: return wv;
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
                case 3: xv = value; break;
                case 4: yz = value; break;
                case 5: yw = value; break;
                case 6: yv = value; break;
                case 7: zw = value; break;
                case 8: zv = value; break;
                case 9: wv = value; break;
            }
        }
    }

    public bool Equals(BiVector5 other)
    {
        return xy == other.xy && xz == other.xz && xw == other.xw && xv == other.xv && yz == other.yz && yw == other.yw && yv == other.yv && zw == other.zw && zv == other.zv && wv == other.wv;
    }

    public override bool Equals(object obj)
    {
        return obj is BiVector5 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(HashCode.Combine(xy, xz, xw, yz, yw), yv, zw, zv, wv);
    }

    public static bool operator ==(BiVector5 left, BiVector5 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BiVector5 left, BiVector5 right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"({xy}, {xz}, {xw}, {xv}, {yz}, {yw}, {yv}, {zw}, {zv}, {wv})";
    }

    public static BiVector5 operator +(BiVector5 a, BiVector5 b)
    {
        return new BiVector5(a.xy + b.xy, a.xz + b.xz, a.xw + b.xw, a.xv + b.xv, a.yz + b.yz, a.yw + b.yw, a.yv + b.yv, a.zw + b.zw, a.zv + b.zv, a.wv + b.wv);
    }

    public static BiVector5 operator -(BiVector5 a, BiVector5 b)
    {
        return new BiVector5(a.xy - b.xy, a.xz - b.xz, a.xw - b.xw, a.xv - b.xv, a.yz - b.yz, a.yw - b.yw, a.yv - b.yv, a.zw - b.zw, a.zv - b.zv, a.wv - b.wv);
    }

    public static BiVector5 operator *(BiVector5 a, float b)
    {
        return new BiVector5(a.xy * b, a.xz * b, a.xw * b, a.xv * b, a.yz * b, a.yw * b, a.yv * b, a.zw * b, a.zv * b, a.wv * b);
    }

    public static BiVector5 operator /(BiVector5 a, float b)
    {
        return new BiVector5(a.xy / b, a.xz / b, a.xw / b, a.xv / b, a.yz / b, a.yw / b, a.yv / b, a.zw / b, a.zv / b, a.wv / b);
    }

    public static Vector5 operator *(Vector5 a, BiVector5 b)
    {
        /*
         *  a is vector : 1-5
         *  b is bivector: 6-15
         *  res[1]=-b[6]*a.y-b.xz*a.z-b.xw*a.w-b.xv*a.v+b[2]
			res[2]=b[6]*a.x-b.yz*a.z-b[11]*a.w-b[12]*a.v
			res[3]=b.xz*a.x+b.yz*a.y-b.zw*a.w-b.zv*a.v
			res[4]=b.xw*a.x+b[11]*a.y+b.zw*a.z-b.wv*a.v
			res[5]=b.xv*a.x+b[12]*a.y+b.zv*a.z+b.wv*a.w
         */
        return new Vector5(
            -b.xy * a.y - b.xz * a.z - b.xw * a.w - b.xv * a.v,
			b.xy * a.x - b.yz * a.z - b.yw * a.w - b.yv * a.v,
			b.xz * a.x + b.yz * a.y - b.zw * a.w - b.zv * a.v,
			b.xw * a.x + b.yw * a.y + b.zw * a.z - b.wv * a.v,
			b.xv * a.x + b.yv * a.y + b.zv * a.z + b.wv * a.w
        );
    }

    public float magnitude => Mathf.Sqrt(xy * xy + xz * xz + xw * xw + xv * xv + yz * yz + yw * yw + yv * yv + zw * zw + zv * zv + wv * wv);

    public float sqrMagnitude => xy * xy + xz * xz + xw * xw + xv * xv + yz * yz + yw * yw + yv * yv + zw * zw + zv * zv + wv * wv;
}
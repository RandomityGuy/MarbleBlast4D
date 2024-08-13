using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface GJKCollider
{
    public Vector4 Support(Vector4 direction);
    Vector4 Position { get; }
}

public class PointCollider4D : GJKCollider
{
    public Vector4 position;
    public PointCollider4D(Vector4 position)
    {
        this.position = position;
    }

    public Vector4 Position => position;

    public Vector4 Support(Vector4 direction)
    {
        return position;
    }
}

public class GJK
{
    static int maxIteration = 64;
    static bool requireWindingFlip = false;

    static Vector4 direction;
    static Vector4[] simplex = new Vector4[5];
    static int numPoints = 0;

    static Vector4 Support(GJKCollider s1, GJKCollider s2, Vector4 direction)
    {
        return s1.Support(direction) - s2.Support(-direction);
    }

    public static bool BallConvexIntersection(SphereCollider4D s1, GJKCollider s2)
    {
        var pointCollider = new PointCollider4D(s1.center);
        var closestPoint = DetermineClosestPoint(s2, pointCollider);
        var distanceSq = closestPoint.sqrMagnitude;
        return distanceSq <= s1.radius * s1.radius;
    }

    static Vector4 DetermineClosestPoint(GJKCollider s1, GJKCollider s2)
    {
        direction = s2.Position - s1.Position;
        numPoints = 0;
        for (var i = 0; i < maxIteration; i++)
        {
            if (numPoints == 5) break;
            var support = Support(s1, s2, direction);
            if (direction.sqrMagnitude + Vector4.Dot(direction, support) <= 0.0001f)
            {
                break;
            }
            AddPointToSimplex(support);
            UpdateSimplexAndClosestPoint(ref direction);
            direction = -direction;
        }
        return -direction;
    }

    static void AddPointToSimplex(Vector4 p)
    {
        for (var i = 0; i < numPoints; i++)
        {
            if ((simplex[i] - p).sqrMagnitude < 0.0001f)
            {
                return;
            }
        }
        // shift all points to the right
        for (var i = numPoints; i > 0; i--)
        {
            simplex[i] = simplex[i - 1];
        }
        // Add the point to the front
        simplex[0] = p;
        numPoints++;
    }
    static void UpdateSimplexAndClosestPoint(ref Vector4 dst)
    {
        var used = 0; // 5 bit number for A B C D E
        requireWindingFlip = false;

        switch (numPoints)
        {
            case 1: 
                used = ClosestPointPoint(ref dst, simplex[0]);
                break;

            case 2:
                used = ClosestPointLineSegment(ref dst, simplex[0], simplex[1]);
                break;

            case 3:
                used = ClosestPointTriangle(ref dst, simplex[0], simplex[1], simplex[2]);
                break;

            case 4:
                used = ClosestPointTetrahedron(ref dst, simplex[0], simplex[1], simplex[2], simplex[3]);
                break;

            case 5:
                used = ClosestPointPentachoron(ref dst, simplex[0], simplex[1], simplex[2], simplex[3], simplex[4]);
                break;

            default:
                throw new Exception("Invalid simplex");
        }

        // Do the whole point assignerino based on whatever points make up the closest feature
        var i = 0;
        for (var j = 0; j < 4; j++)
        {
            if ((used & (1 << j)) > 0)
            {
                simplex[i] = simplex[j];
                i++;
            }
        }
        numPoints = i;

        // Flip winding order of triangle if needed
        if (requireWindingFlip)
        {
            var temp = simplex[1];
            simplex[1] = simplex[2];
            simplex[2] = temp;
        }
    }
    static int ClosestPointPoint(ref Vector4 dst, Vector4 a)
    {
        dst.Set(a.x, a.y, a.z, a.w);
        return 0b00001;
    }

    static int ClosestPointLineSegment(ref Vector4 dst, Vector4 a, Vector4 b)
    {
        /*          /
                   A
                  / \
                 /   \
                /     \     / 
               /       \   / 
                        \ /
              O          B
                        /
                       /
         */
        var ab = b - a;
        var ao = -a;

        var t = Mathf.Clamp01(Vector4.Dot(ao, ab) / Vector4.Dot(ab, ab));
        if (t < 0.0001f)
        {
            dst.Set(a.x, a.y, a.z, a.w);
            return 0b00001;
        }
        else if (t > 0.9999f)
        {
            dst.Set(b.x, b.y, b.z, b.w);
            return 0b00010;
        }
        else
        {
            dst.Set(a.x + ab.x * t, a.y + ab.y * t, a.z + ab.z * t, a.w + ab.w * t);
            return 0b00011;
        }
    }

    static int ClosestPointTriangle(ref Vector4 dst, Vector4 a, Vector4 b, Vector4 c)
    {
        /*
         * 
         *                         #1
         *                       \   /
         *                        \ /
         *                         A
         *                        / \
         *                 #3    /   \   #5
         *                      /     \
         *                     /       \
         *                    /         \
         *               --- B --------- C ---
         *             #2   /      #6     \   #4
         *                 /               \
         * 
         */

        var ab = b - a;
        var ac = c - a;
        var ao = -a;

        var abao = Vector4.Dot(ab, ao); // d1
        var acao = Vector4.Dot(ac, ao); // d2
        if (abao <= 0 && acao <= 0) // #1
        {
            dst.Set(a.x, a.y, a.z, a.w);
            return 0b00001;
        }
        var bo = -b;
        var abbo = Vector4.Dot(ab, bo); // d3
        var acbo = Vector4.Dot(ac, bo); // d4
        if (abbo >= 0 && acbo <= abbo) // #2
        {
            dst.Set(b.x, b.y, b.z, b.w);
            return 0b00010;
        }

        var vc = abao * acbo - acao * abbo;
        if (vc <= 0 && abao >= 0 && abbo <= 0) // #3
        {
            var t = abao / (abao - abbo);
            dst.Set(a.x + ab.x * t, a.y + ab.y * t, a.z + ab.z * t, a.w + ab.w * t);
            return 0b00011;
        }

        var co = -c;
        var abco = Vector4.Dot(ab, co); // d5
        var acco = Vector4.Dot(ac, co); // d6
        if (abco >= 0 && acco >= abco) // #4
        {
            dst.Set(c.x, c.y, c.z, c.w);
            return 0b00100;
        }

        var vb = abco * acao - abao * acco;
        if (vb <= 0 && abco >= 0 && acco <= 0) // #5
        {
            var t = abco / (abco - acco);
            dst.Set(a.x + ac.x * t, a.y + ac.y * t, a.z + ac.z * t, a.w + ac.w * t);
            return 0b00101;
        }

        var va = abbo * acco - abco * acbo;
        if (va <= 0 && (acbo - abbo) >= 0 && (abco - acco) <= 0) // #6
        {
            var bc = c - b;
            var t = (acbo - abbo) / (acbo - abbo + abco - acco);
            dst.Set(b.x + bc.x * t, b.y + bc.y * t, b.z + bc.z * t, b.w + bc.w * t);
            return 0b00110;
        }

        var denom = 1 / (va + vb + vc);
        var v = vb * denom;
        var w = vc * denom;

        dst.Set(a.x + ab.x * v + ac.x * w, a.y + ab.y * v + ac.y * w, a.z + ab.z * v + ac.z * w, a.w + ab.w * v + ac.w * w);
        var abc = Transform4D.ExteriorProduct(ab, ac);
        if (abc.OuterMagnitude(dst) > 0) {
            requireWindingFlip = !requireWindingFlip;
        }

        return 0b00111;
    }

    static int ClosestPointTetrahedron(ref Vector4 dst, Vector4 a, Vector4 b, Vector4 c, Vector4 d)
    {
        /*
         
                                 A
                                /| \
                               / |   \
                              /  |     \
                             /   |       \
                            /    |         \
                           /     |           \
                          /      |             \
                         /       |               \
                        /        D                 \
                       /        /      \             \
                      /      /                \        \
                     /   /                           \   \
                     B  --------------------------------- C





        */
        var ab = b - a;
        var ac = c - a;
        var ad = d - a;
        var bc = c - b;
        var bd = c - d;
        var ao = -a;
        var bo = -b;

        var abc = Transform4D.ExteriorProduct(ab, ac);
        var acd = Transform4D.ExteriorProduct(ac, ad);
        var adb = Transform4D.ExteriorProduct(ad, ab);
        var bdc = Transform4D.ExteriorProduct(bd, bc);

        var abcao = abc.OuterMagnitude(ao);
        var acdao = acd.OuterMagnitude(ao);
        var adbao = adb.OuterMagnitude(ao);
        var bdcbo = bdc.OuterMagnitude(bo);

        var outsideABC = abcao > 0;
        var outsideACD = acdao > 0;
        var outsideADB = adbao > 0;
        var outsideBDC = bdcbo > 0;

        var minDist = 1e8f;
        var used = 0;
        var flip = false;

        if (outsideABC)
        {
            requireWindingFlip = false;
            var temp = new Vector4();
            var res = ClosestPointTriangle(ref temp, a, b, c);
            var len = temp.sqrMagnitude;
            dst.Set(temp.x, temp.y, temp.z, temp.w);
            minDist = len;
            used = res;
            flip = requireWindingFlip;
        }
        if (outsideACD)
        {
            requireWindingFlip = false;
            var temp = new Vector4();
            var res = ClosestPointTriangle(ref temp, a, c, d);
            var len = temp.sqrMagnitude;
            if (len < minDist)
            {
                dst.Set(temp.x, temp.y, temp.z, temp.w);
                minDist = len;
                used = (res & 0b1) | ((res & 0b10) << 1) | ((res & 0b100) << 1); // Shift the bits around so they match the passed points
                flip = requireWindingFlip;
            }
        }
        if (outsideADB)
        {
            requireWindingFlip = false;
            var temp = new Vector4();
            var res = ClosestPointTriangle(ref temp, a, d, b);
            var len = temp.sqrMagnitude;
            if (len < minDist)
            {
                dst.Set(temp.x, temp.y, temp.z, temp.w);
                minDist = len;
                used = (res & 0b1) | ((res & 0b10) << 2) | ((res & 0b100) >> 1); // Shift the bits around so they match the passed points
                flip = !requireWindingFlip;
            }
        }
        if (outsideBDC)
        {
            requireWindingFlip = false;
            var temp = new Vector4();
            var res = ClosestPointTriangle(ref temp, b, d, c);
            var len = temp.sqrMagnitude;
            if (len < minDist)
            {
                dst.Set(temp.x, temp.y, temp.z, temp.w);
                minDist = len;
                used = ((res & 0b1) << 1) | ((res & 0b10) << 2) | ((res & 0b100)); // Shift the bits around so they match the passed points
                flip = !requireWindingFlip;
            }
        }

        // Final search direction for tetrahedron

        if (minDist == 1e8f)
        {
            used = 0b01111;
            flip = false;
            return used;
        } 
        else
        {
            requireWindingFlip = flip;
            return used;
        }

        return 0b01000;
    }

    static int ClosestPointPentachoron(ref Vector4 dst, Vector4 a, Vector4 b, Vector4 c, Vector4 d, Vector4 e)
    {
        var ab = b - a;
        var ac = c - a;
        var ad = d - a;
        var ae = e - a;
        var bc = c - b;
        var bd = d - b;
        var be = e - b;
        var cd = d - c;
        var ce = e - c;
        var de = e - d;
        return 0b10000;
    }
}


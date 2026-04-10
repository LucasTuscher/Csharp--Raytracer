using System;
using System.Collections.Generic;
using System.IO;

namespace SimpleRaytracer
{
    // Vektormathematik
    public struct Vector
    {
        public double X, Y, Z;
        public Vector(double x, double y, double z) { X = x; Y = y; Z = z; }
        public static Vector operator +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector operator -(Vector a, Vector b) => new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector operator *(Vector a, double d) => new Vector(a.X * d, a.Y * d, a.Z * d);
        public static Vector operator *(Vector a, Vector b) => new Vector(a.X * b.X, a.Y * b.Y, a.Z * b.Z); // Farbmultiplikation
        public static Vector operator /(Vector a, double d) => new Vector(a.X / d, a.Y / d, a.Z / d);
        public static double Dot(Vector a, Vector b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public Vector Normalize()
        {
            double mag = Math.Sqrt(X * X + Y * Y + Z * Z);
            return mag == 0 ? new Vector(0, 0, 0) : this / mag;
        }
    }

    public class Ray
    {
        public Vector Origin, Direction;
        public Ray(Vector origin, Vector direction) { Origin = origin; Direction = direction.Normalize(); }
    }

    public class Material
    {
        public Vector Color;
        public double Reflection, Diffuse, Specular, Shininess;
        public Material(Vector color, double reflection, double diffuse, double specular, double shininess)
        {
            Color = color; Reflection = reflection; Diffuse = diffuse; Specular = specular; Shininess = shininess;
        }
    }

    // Geometrie-Schnittpunkt
    public class Intersection
    {
        public SceneObject Object;
        public double Distance;
        public Intersection(SceneObject obj, double distance) { Object = obj; Distance = distance; }
    }

    public abstract class SceneObject
    {
        public Material Material;
        public abstract Intersection Intersect(Ray ray);
        public abstract Vector GetNormal(Vector point);
        public abstract Vector GetColor(Vector point); // Erlaubt Muster
    }

    public class Sphere : SceneObject
    {
        public Vector Center;
        public double Radius;
        public Sphere(Vector center, double radius, Material material) { Center = center; Radius = radius; Material = material; }

        public override Intersection Intersect(Ray ray)
        {
            Vector L = Center - ray.Origin;
            double tca = Vector.Dot(L, ray.Direction);
            if (tca < 0) return null;
            double d2 = Vector.Dot(L, L) - tca * tca;
            if (d2 > Radius * Radius) return null;
            double thc = Math.Sqrt(Radius * Radius - d2);
            double t0 = tca - thc;
            double t1 = tca + thc;
            if (t0 < 0) t0 = t1;
            if (t0 < 0) return null;
            return new Intersection(this, t0);
        }

        public override Vector GetNormal(Vector point) => (point - Center).Normalize();
        public override Vector GetColor(Vector point) => Material.Color;
    }

    public class Plane : SceneObject
    {
        public Vector Normal;
        public double Distance;
        public Plane(Vector normal, double distance, Material material) { Normal = normal.Normalize(); Distance = distance; Material = material; }

        public override Intersection Intersect(Ray ray)
        {
            double denom = Vector.Dot(Normal, ray.Direction);
            if (Math.Abs(denom) > 1e-6)
            {
                double t = (Distance - Vector.Dot(Normal, ray.Origin)) / denom;
                if (t >= 0) return new Intersection(this, t);
            }
            return null;
        }

        public override Vector GetNormal(Vector point) => Normal;
        public override Vector GetColor(Vector point)
        {
            // Schachbrettmuster für den Boden
            int check = (int)(Math.Floor(point.X * 0.5) + Math.Floor(point.Z * 0.5));
            return (check % 2 == 0) ? Material.Color : new Vector(1, 1, 1);
        }
    }

    public class Light
    {
        public Vector Position, Color;
        public double Intensity;
        public Light(Vector position, Vector color, double intensity) { Position = position; Color = color; Intensity = intensity; }
    }

    // Der eigentliche Raytracer
    class Program
    {
        static List<SceneObject> objects = new List<SceneObject>();
        static List<Light> lights = new List<Light>();

        static void Main(string[] args)
        {
            int width = 800;
            int height = 600;
            int maxDepth = 4; // Maximale Anzahl der Reflexionen

            // Szene aufbauen
            objects.Add(new Plane(new Vector(0, 1, 0), -2, new Material(new Vector(0.2, 0.2, 0.2), 0.3, 0.7, 0.2, 10)));
            objects.Add(new Sphere(new Vector(-2, 0, 5), 2, new Material(new Vector(0.8, 0.2, 0.2), 0.1, 0.8, 0.8, 50))); // Rote Kugel
            objects.Add(new Sphere(new Vector(2, 0, 6), 2, new Material(new Vector(0.2, 0.2, 0.8), 0.5, 0.5, 0.8, 50))); // Blaue Kugel (Spiegelnd)
            objects.Add(new Sphere(new Vector(0, -1, 3), 1, new Material(new Vector(0.2, 0.8, 0.2), 0.1, 0.8, 0.5, 30))); // Grüne Kugel

            lights.Add(new Light(new Vector(-5, 10, -5), new Vector(1, 1, 1), 1.0));
            lights.Add(new Light(new Vector(5, 10, -5), new Vector(0.5, 0.5, 0.8), 0.5));

            Vector camera = new Vector(0, 1, -5);
            double fov = Math.PI / 3.0; // 60 Grad Sichtfeld

            Console.WriteLine("Rendere Bild...");
            using (StreamWriter writer = new StreamWriter("output.ppm"))
            {
                // PPM Header
                writer.WriteLine($"P3\n{width} {height}\n255");

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double dirX = (x + 0.5) - width / 2.0;
                        double dirY = -(y + 0.5) + height / 2.0; // Y nach oben
                        double dirZ = -(height / 2.0) / Math.Tan(fov / 2.0);

                        Ray ray = new Ray(camera, new Vector(dirX, dirY, -dirZ));
                        Vector color = TraceRay(ray, 0, maxDepth);

                        // Farbwerte clippen und ausgeben
                        int r = (int)(Math.Clamp(color.X, 0, 1) * 255);
                        int g = (int)(Math.Clamp(color.Y, 0, 1) * 255);
                        int b = (int)(Math.Clamp(color.Z, 0, 1) * 255);
                        writer.WriteLine($"{r} {g} {b}");
                    }
                }
            }
            Console.WriteLine("Fertig! Bild gespeichert als 'output.ppm'.");
        }

        static Vector TraceRay(Ray ray, int depth, int maxDepth)
        {
            Intersection closestHit = null;
            double minDistance = double.MaxValue;

            // Finde das nächste Objekt
            foreach (var obj in objects)
            {
                Intersection hit = obj.Intersect(ray);
                if (hit != null && hit.Distance < minDistance)
                {
                    minDistance = hit.Distance;
                    closestHit = hit;
                }
            }

            if (closestHit == null) return new Vector(0.05, 0.05, 0.05); // Hintergrundfarbe (Dunkelgrau)

            Vector point = ray.Origin + ray.Direction * closestHit.Distance;
            Vector normal = closestHit.Object.GetNormal(point);
            Vector objColor = closestHit.Object.GetColor(point);
            Material mat = closestHit.Object.Material;

            Vector finalColor = new Vector(0, 0, 0);

            // Beleuchtung berechnen
            foreach (var light in lights)
            {
                Vector lightDir = (light.Position - point).Normalize();
                double distanceToLight = Math.Sqrt(Vector.Dot(light.Position - point, light.Position - point));

                // Schattenwurf
                Ray shadowRay = new Ray(point + normal * 1e-4, lightDir);
                bool inShadow = false;
                foreach (var shadowObj in objects)
                {
                    Intersection shadowHit = shadowObj.Intersect(shadowRay);
                    if (shadowHit != null && shadowHit.Distance < distanceToLight)
                    {
                        inShadow = true;
                        break;
                    }
                }

                if (!inShadow)
                {
                    // Diffuses Licht (Lambert)
                    double nDotL = Math.Max(0, Vector.Dot(normal, lightDir));
                    Vector diffuse = objColor * light.Color * mat.Diffuse * nDotL * light.Intensity;

                    // Spiegelglanz (Phong)
                    Vector viewDir = (ray.Origin - point).Normalize();
                    Vector reflectDir = (lightDir - normal * 2 * Vector.Dot(lightDir, normal)).Normalize() * -1;
                    double specBase = Math.Max(0, Vector.Dot(viewDir, reflectDir));
                    double specPower = Math.Pow(specBase, mat.Shininess);
                    Vector specular = light.Color * mat.Specular * specPower * light.Intensity;

                    finalColor += diffuse + specular;
                }
            }

            // Umgebungslicht (Ambient)
            finalColor += objColor * 0.1;

            // Reflexionen (Rekursion)
            if (depth < maxDepth && mat.Reflection > 0)
            {
                Vector reflectDir = ray.Direction - normal * 2 * Vector.Dot(ray.Direction, normal);
                Ray reflectRay = new Ray(point + normal * 1e-4, reflectDir);
                Vector reflectColor = TraceRay(reflectRay, depth + 1, maxDepth);
                finalColor = finalColor * (1 - mat.Reflection) + reflectColor * mat.Reflection;
            }

            return finalColor;
        }
    }
}

# Simple C# Raytracer

Ein minimalistischer, vollständiger Raytracer in C#, der komplett in eine einzige Datei (`Program.cs`) passt. Dieses Projekt demonstriert die grundlegenden Konzepte der Computergrafik und des Raytracings, ohne dass externe Grafikbibliotheken benötigt werden.

## Features

* **Geometrie:** Rendert Kugeln (Spheres) und eine unendliche Ebene (Plane).
* **Beleuchtung (Phong-Modell):** Unterstützt diffuses Licht, spekulare Glanzpunkte (Spiegelungen auf der Oberfläche) und weiches Umgebungslicht (Ambient).
* **Schatten:** Harte Schattenwürfe (Hard Shadows) durch Ray-Casting zu den Lichtquellen.
* **Reflexionen:** Rekursive Raytracing-Logik für echte Spiegelungen zwischen Objekten.
* **Materialien:** Prozedurales Schachbrettmuster für den Boden und individuell einstellbare Farben und Reflexionsgrade für die Kugeln.
* **PPM-Export:** Schreibt das gerenderte Bild direkt als `.ppm`-Datei (Portable Pixmap), was plattformunabhängig und leicht zu verstehen ist.

## Ausführen des Projekts

Stelle sicher, dass du das [.NET SDK](https://dotnet.microsoft.com/download) auf deinem System installiert hast.

1.  Öffne ein Terminal oder die Kommandozeile.
2.  Erstelle ein neues Konsolenprojekt (falls noch nicht geschehen):
    ```bash
    dotnet new console -n SimpleRaytracer
    cd SimpleRaytracer
    ```
3.  Ersetze den Inhalt der `Program.cs` mit dem Raytracer-Code.
4.  Starte das Rendering:
    ```bash
    dotnet run
    ```
5.  Nach wenigen Sekunden sollte in der Konsole die Meldung `Fertig! Bild gespeichert als 'output.ppm'.` erscheinen.

## Wie öffne ich die .ppm-Datei?

Das PPM-Format (Portable Pixmap) ist ein unkomprimiertes Text-Bildformat. 
* **Windows:** [IrfanView](https://www.irfanview.com/) ist hervorragend geeignet, um `.ppm`-Dateien blitzschnell zu öffnen. GIMP oder Photoshop funktionieren ebenfalls.
* **macOS / Linux:** Viele integrierte Bildbetrachter (wie `Preview` auf dem Mac) oder Programme wie GIMP öffnen diese Dateien nativ.
* **Online:** Du kannst auch nach einem "PPM to PNG Converter online" suchen und die Datei im Browser umwandeln lassen.

## Eigene Experimente

Du kannst den Code leicht anpassen, um zu lernen, wie der Raytracer funktioniert:

* **Auflösung ändern:** Ändere `int width = 800;` und `int height = 600;` in der `Main`-Methode für größere Bilder (Achtung: das Rendern dauert dann länger).
* **Reflexionen anpassen:** Erhöhe `int maxDepth = 4;`, um zu sehen, wie Licht öfter zwischen den Kugeln hin und her spiegelt.
* **Szene umbauen:** Füge neue `Sphere`-Objekte in der Liste `objects.Add(...)` hinzu oder ändere die Positionen (`Vector`) der bestehenden Lichter und Kugeln.

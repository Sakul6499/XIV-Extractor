using SaintCoinach.Graphics;
using SaintCoinach.Graphics.Viewer;
using SharpDX;

namespace SaintCoinachWrapper
{
    public class Exporter
    {
        static Matrix IdentityMatrix = Matrix.Identity;

        /**
         * Exports a Territory and returns `true` if successful, `false` otherwise.
         * 
         * @param {Client} client - The Wrapper client.
         * @param {Territory} territory - The territory to export.
         */
        public static void Export(Client client, Territory territory, String output_folder = "output/", bool write_to_console = true)
        {
            var exportDirectory = $"{output_folder}{client.TerritoryToFullString(territory).ToLower().Replace(" ", "_")}/";

            if (!output_folder.EndsWith("/"))
            {
                output_folder += "/";
            }

            if (write_to_console)
            {
                Console.WriteLine($"Exporting '{client.TerritoryToFullString(territory)}' to '{exportDirectory}'.");
            }

            // Check & Create output folder
            if (!System.IO.Directory.Exists($"{exportDirectory}"))
            {
                System.IO.Directory.CreateDirectory($"{exportDirectory}");
            }

            var territoryFileName = $"{exportDirectory}{territory.Name}.obj";
            var lightsFileName = $"{exportDirectory}{territory.Name}-lights.txt";

            System.IO.File.Create(territoryFileName).Close();
            System.IO.File.AppendAllText(territoryFileName, $"o {territory.Name}\n");
            System.IO.File.WriteAllText(lightsFileName, "");

            int lights = 0;
            List<string> light_data = new List<string>() { "import bpy" };

            List<string> vertex_data = new List<string>();

            Dictionary<string, bool> exportedPaths = new Dictionary<string, bool>();
            UInt64 vs = 1, vt = 1, vn = 1, i = 0;
            Matrix IdentityMatrix = Matrix.Identity;

            void ExportMaterials(Material m, string path)
            {
                vertex_data.Add($"mtllib {path}.mtl");
                bool found = false;
                if (exportedPaths.TryGetValue(path, out found))
                {
                    return;
                }
                exportedPaths.Add(path, true);
                System.IO.File.Delete($"{exportDirectory}/{path}.mtl");
                System.IO.File.AppendAllText($"{exportDirectory}/{path}.mtl", $"newmtl {path}\n");
                foreach (var img in m.TexturesFiles)
                {
                    var mtlName = img.Path.Replace('/', '_');
                    if (exportedPaths.TryGetValue(path + mtlName, out found))
                    {
                        continue;
                    }

                    if (mtlName.Contains("_dummy_"))
                        continue;

                    var ddsBytes = SaintCoinach.Imaging.ImageConverter.GetDDS(img);

                    var fileExt = ddsBytes != null ? ".dds" : ".png";
                    if (fileExt == ".dds")
                        System.IO.File.WriteAllBytes($"{exportDirectory}/{mtlName}.dds", ddsBytes!);
                    else
                        SaintCoinach.Imaging.ImageConverter.Convert(img).Save($"{exportDirectory}/{mtlName}.png");


                    if (mtlName.Contains("_n.tex"))
                    {
                        System.IO.File.AppendAllText($"{exportDirectory}{path}.mtl", $"bump {mtlName}{fileExt}\n");
                    }
                    else if (mtlName.Contains("_s.tex"))
                    {
                        System.IO.File.AppendAllText($"{exportDirectory}{path}.mtl", $"map_Ks {mtlName}{fileExt}\n");
                    }
                    else if (!mtlName.Contains("_a.tex"))
                    {
                        System.IO.File.AppendAllText($"{exportDirectory}{path}.mtl", $"map_Kd {mtlName}{fileExt}\n");
                    }
                    else
                    {
                        System.IO.File.AppendAllText($"{exportDirectory}{path}.mtl", $"map_Ka {mtlName}{fileExt}\n");
                    }

                    exportedPaths.Add(path + mtlName, true);
                }
            }

            Matrix CreateMatrix(SaintCoinach.Graphics.Vector3 translation, SaintCoinach.Graphics.Vector3 rotation, SaintCoinach.Graphics.Vector3 scale)
            {
                return (Matrix.Scaling(scale.ToDx())
                    * Matrix.RotationX(rotation.X)
                    * Matrix.RotationY(rotation.Y)
                    * Matrix.RotationZ(rotation.Z)
                    * Matrix.Translation(translation.ToDx()));
            }

            void ExportMesh(ref Mesh mesh, ref Matrix lgbTransform, ref string materialName, ref string modelFilePath,
                ref Matrix rootGimTransform, ref Matrix currGimTransform, ref Matrix modelTransform)
            {
                i++;
                var k = 0;
                UInt64 tempVs = 0, tempVn = 0, tempVt = 0;
                foreach (var v in mesh.Vertices)
                {

                    var x = v.Position!.Value.X;
                    var y = v.Position!.Value.Y;
                    var z = v.Position!.Value.Z;
                    var w = v.Position!.Value.W;

                    var transform = (modelTransform * rootGimTransform * currGimTransform) * lgbTransform;

                    var t = Matrix.Translation(x, y, z) * transform;
                    x = t.TranslationVector!.X;
                    y = t.TranslationVector!.Y;
                    z = t.TranslationVector!.Z;

                    if (v.Color != null)
                        vertex_data.Add($"v {x} {y} {z} {v.Color.Value.X} {v.Color.Value.Y} {v.Color.Value.Z} {v.Color.Value.W}".Replace(',', '.'));
                    else
                        vertex_data.Add($"v {x} {y} {z}".Replace(',', '.'));

                    tempVs++;

                    vertex_data.Add($"vn {v.Normal!.Value.X} {v.Normal!.Value.Y} {v.Normal!.Value.Z}".Replace(',', '.'));
                    tempVn++;

                    if (v.UV != null)
                    {
                        vertex_data.Add($"vt {v.UV.Value.X} {v.UV.Value.Y * -1.0}".Replace(',', '.'));
                        tempVt++;
                    }
                }
                vertex_data.Add($"g {modelFilePath}_{i.ToString()}_{k.ToString()}");
                vertex_data.Add($"usemtl {materialName}");
                for (UInt64 j = 0; j + 3 < (UInt64)mesh.Indices.Length + 1; j += 3)
                {
                    vertex_data.Add(
                        $"f " +
                        $"{mesh.Indices[j] + vs}/{mesh.Indices[j] + vt}/{mesh.Indices[j] + vn} " +
                        $"{mesh.Indices[j + 1] + vs}/{mesh.Indices[j + 1] + vt}/{mesh.Indices[j + 1] + vn} " +
                        $"{mesh.Indices[j + 2] + vs}/{mesh.Indices[j + 2] + vt}/{mesh.Indices[j + 2] + vn}");
                }
                if (i % 1000 == 0)
                {
                    System.IO.File.AppendAllLines(territoryFileName, vertex_data);
                    vertex_data.Clear();
                }
                vs += tempVs;
                vn += tempVn;
                vt += tempVt;
            }

            Dictionary<string, bool> exportedSgbFiles = new Dictionary<string, bool>();
            void ExportSgbModels(SaintCoinach.Graphics.Sgb.SgbFile sgbFile, ref Matrix lgbTransform, ref Matrix rootGimTransform, ref Matrix currGimTransform)
            {
                foreach (var sgbGroup in sgbFile.Data.OfType<SaintCoinach.Graphics.Sgb.SgbGroup>())
                {
                    bool newGroup = true;

                    foreach (var sgb1CEntry in sgbGroup.Entries.OfType<SaintCoinach.Graphics.Sgb.SgbGroup1CEntry>())
                    {
                        if (sgb1CEntry.Gimmick != null)
                        {
                            ExportSgbModels(sgb1CEntry.Gimmick, ref lgbTransform, ref IdentityMatrix, ref IdentityMatrix);
                            foreach (var subGimGroup in sgb1CEntry.Gimmick.Data.OfType<SaintCoinach.Graphics.Sgb.SgbGroup>())
                            {
                                foreach (var subGimEntry in subGimGroup.Entries.OfType<SaintCoinach.Graphics.Sgb.SgbGimmickEntry>())
                                {
                                    var subGimTransform = CreateMatrix(subGimEntry.Header.Translation, subGimEntry.Header.Rotation, subGimEntry.Header.Scale);
                                    ExportSgbModels(subGimEntry.Gimmick, ref lgbTransform, ref IdentityMatrix, ref subGimTransform);
                                }
                            }
                        }
                    }

                    foreach (var mdl in sgbGroup.Entries.OfType<SaintCoinach.Graphics.Sgb.SgbModelEntry>())
                    {
                        var filePath = mdl.ModelFilePath;
                        var modelTransform = CreateMatrix(mdl.Header.Translation, mdl.Header.Rotation, mdl.Header.Scale);

                        Model hq;
                        try
                        {
                            hq = mdl.Model.Model.GetModel(ModelQuality.High);
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine($"Unable to load model for {mdl.Name} path: {filePath}.  Exception: {e.Message}");
                            continue;
                        }
                        if (newGroup)
                        {
                            newGroup = false;
                        }
                        for (var j = 0; j < hq.Meshes.Length; ++j)
                        {
                            var mesh = hq.Meshes[j];
                            var mtl = mesh.Material.Get();
                            var path = mtl.File.Path.Replace('/', '_').Replace(".mtrl", ".tex");

                            ExportMaterials(mtl, path);
                            ExportMesh(ref mesh, ref lgbTransform, ref path, ref filePath, ref rootGimTransform, ref currGimTransform, ref modelTransform);
                        }
                    }

                    foreach (var light in sgbGroup.Entries.OfType<SaintCoinach.Graphics.Sgb.SgbLightEntry>())
                    {
                        var pos = light.Header.Translation;
                        var transform = (Matrix.Translation(pos.X, pos.Y, pos.Z) * (rootGimTransform * currGimTransform) * lgbTransform).TranslationVector;
                        pos.X = transform.X;
                        pos.Y = transform.Y;
                        pos.Z = transform.Z;

                        light_data.Add($"#LIGHT_{lights++}_{light.Name}_{light.Header.UnknownId}");
                        light_data.Add($"#pos {pos.X} {pos.Y} {pos.Z}");
                        light_data.Add($"#UNKNOWNFLAGS 0x{light.Header.UnknownFlag1:X8} 0x{light.Header.UnknownFlag2:X8} 0x{light.Header.UnknownFlag3:X8} 0x{light.Header.UnknownFlag4:X8}");
                        light_data.Add($"#UNKNOWN {light.Header.Rotation.X} {light.Header.Rotation.Y} {light.Header.Rotation.Z}");
                        light_data.Add($"#UNKNOWN2 {light.Header.Scale.X} {light.Header.Scale.Y} {light.Header.Scale.Z}");
                        light_data.Add($"#unk {light.Header.Entry1.X} {light.Header.Entry1.Y}");
                        light_data.Add($"#unk2 {light.Header.Entry2.X} {light.Header.Entry2.Y}");
                        light_data.Add($"#unk3 {light.Header.Entry3.X} {light.Header.Entry3.Y}");
                        light_data.Add($"#unk4 {light.Header.Entry4.X} {light.Header.Entry4.Y}");
                        light_data.Add("");
                    }
                }
            }

            if (territory.Terrain != null)
            {
                foreach (var part in territory.Terrain.Parts)
                {
                    if (write_to_console)
                    {
                        Console.Out.Write(".");
                        Console.Out.Flush();
                    }

                    var hq = part.Model.GetModel(ModelQuality.High);
                    var filePath = hq.Definition.File.Path;
                    var lgbTransform = CreateMatrix(part.Translation, part.Rotation, part.Scale);

                    for (var j = 0; j < hq.Meshes.Length; ++j)
                    {
                        var mesh = hq.Meshes[j];
                        var mtl = mesh.Material.Get();
                        var path = mtl.File.Path.Replace('/', '_').Replace(".mtrl", ".tex");

                        ExportMaterials(mtl, path);
                        ExportMesh(ref mesh, ref lgbTransform, ref path, ref filePath, ref IdentityMatrix, ref IdentityMatrix, ref IdentityMatrix);
                    }
                }
            }

            System.IO.File.AppendAllLines(territoryFileName, vertex_data);
            vertex_data.Clear();
            vs = 1; vn = 1; vt = 1; i = 0;
            foreach (var lgb in territory.LgbFiles)
            {
                foreach (var lgbGroup in lgb.Groups)
                {

                    bool newGroup = true;
                    foreach (var part in lgbGroup.Entries)
                    {
                        if (part == null)
                            continue;

                        if (write_to_console)
                        {
                            Console.Out.Write(".");
                            Console.Out.Flush();
                        }

                        if (newGroup && (part.Type == SaintCoinach.Graphics.Lgb.LgbEntryType.Model || part.Type == SaintCoinach.Graphics.Lgb.LgbEntryType.Gimmick || part.Type == SaintCoinach.Graphics.Lgb.LgbEntryType.Light))
                        {
                            newGroup = false;

                            System.IO.File.AppendAllLines(territoryFileName, vertex_data);
                            vertex_data.Clear();

                            vs = 1; vn = 1; vt = 1; i = 0;
                            territoryFileName = $"{exportDirectory}{territory.Name}-{lgbGroup.Name}.obj";
                            lightsFileName = $"{exportDirectory}{territory.Name}-{lgbGroup.Name}-lights.txt";

                            var f = System.IO.File.Create(territoryFileName);
                            f.Close();
                            f = System.IO.File.Create(lightsFileName);
                            f.Close();
                        }

                        switch (part.Type)
                        {
                            case SaintCoinach.Graphics.Lgb.LgbEntryType.Model:
                                var asMdl = part as SaintCoinach.Graphics.Lgb.LgbModelEntry;

                                if (asMdl!.Model == null)
                                    continue;

                                var hq = asMdl.Model.Model.GetModel(ModelQuality.High);
                                var lgbTransform = CreateMatrix(asMdl.Header.Translation, asMdl.Header.Rotation, asMdl.Header.Scale);
                                var filePath = asMdl.ModelFilePath;

                                for (var j = 0; j < hq.Meshes.Length; ++j)
                                {
                                    var mesh = hq.Meshes[j];
                                    var mtl = mesh.Material.Get();
                                    var path = mtl.File.Path.Replace('/', '_').Replace(".mtrl", ".tex");

                                    ExportMaterials(mtl, path);
                                    ExportMesh(ref mesh, ref lgbTransform, ref path, ref filePath, ref IdentityMatrix, ref IdentityMatrix, ref IdentityMatrix);
                                }
                                break;
                            case SaintCoinach.Graphics.Lgb.LgbEntryType.Gimmick:
                                var asGim = part as SaintCoinach.Graphics.Lgb.LgbGimmickEntry;
                                if (asGim!.Gimmick == null)
                                    continue;


                                lgbTransform = CreateMatrix(asGim.Header.Translation, asGim.Header.Rotation, asGim.Header.Scale);

                                ExportSgbModels(asGim.Gimmick, ref lgbTransform, ref IdentityMatrix, ref IdentityMatrix);
                                foreach (var rootGimGroup in asGim.Gimmick.Data.OfType<SaintCoinach.Graphics.Sgb.SgbGroup>())
                                {
                                    foreach (var rootGimEntry in rootGimGroup.Entries.OfType<SaintCoinach.Graphics.Sgb.SgbGimmickEntry>())
                                    {
                                        if (rootGimEntry.Gimmick != null)
                                        {
                                            var rootGimTransform = CreateMatrix(rootGimEntry.Header.Translation, rootGimEntry.Header.Rotation, rootGimEntry.Header.Scale);
                                            ExportSgbModels(rootGimEntry.Gimmick, ref lgbTransform, ref rootGimTransform, ref IdentityMatrix);
                                            foreach (var subGimGroup in rootGimEntry.Gimmick.Data.OfType<SaintCoinach.Graphics.Sgb.SgbGroup>())
                                            {
                                                foreach (var subGimEntry in subGimGroup.Entries.OfType<SaintCoinach.Graphics.Sgb.SgbGimmickEntry>())
                                                {
                                                    var subGimTransform = CreateMatrix(subGimEntry.Header.Translation, subGimEntry.Header.Rotation, subGimEntry.Header.Scale);
                                                    ExportSgbModels(subGimEntry.Gimmick, ref lgbTransform, ref rootGimTransform, ref subGimTransform);
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case SaintCoinach.Graphics.Lgb.LgbEntryType.EventObject:
                                var asEobj = part as SaintCoinach.Graphics.Lgb.LgbEventObjectEntry;
                                if (asEobj!.Gimmick == null)
                                    continue;


                                lgbTransform = CreateMatrix(asEobj.Header.Translation, asEobj.Header.Rotation, asEobj.Header.Scale);

                                ExportSgbModels(asEobj.Gimmick, ref lgbTransform, ref IdentityMatrix, ref IdentityMatrix);
                                foreach (var rootGimGroup in asEobj.Gimmick.Data.OfType<SaintCoinach.Graphics.Sgb.SgbGroup>())
                                {
                                    foreach (var rootGimEntry in rootGimGroup.Entries.OfType<SaintCoinach.Graphics.Sgb.SgbGimmickEntry>())
                                    {
                                        if (rootGimEntry.Gimmick != null)
                                        {
                                            var rootGimTransform = CreateMatrix(rootGimEntry.Header.Translation, rootGimEntry.Header.Rotation, rootGimEntry.Header.Scale);
                                            ExportSgbModels(rootGimEntry.Gimmick, ref lgbTransform, ref rootGimTransform, ref IdentityMatrix);
                                            foreach (var subGimGroup in rootGimEntry.Gimmick.Data.OfType<SaintCoinach.Graphics.Sgb.SgbGroup>())
                                            {
                                                foreach (var subGimEntry in subGimGroup.Entries.OfType<SaintCoinach.Graphics.Sgb.SgbGimmickEntry>())
                                                {
                                                    var subGimTransform = CreateMatrix(subGimEntry.Header.Translation, subGimEntry.Header.Rotation, subGimEntry.Header.Scale);
                                                    ExportSgbModels(subGimEntry.Gimmick, ref lgbTransform, ref rootGimTransform, ref subGimTransform);
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case SaintCoinach.Graphics.Lgb.LgbEntryType.Light:
                                var asLight = part as SaintCoinach.Graphics.Lgb.LgbLightEntry;
                                light_data.Add($"#LIGHT_{lights++}_{asLight!.Name}_{asLight!.Header.UnknownId}");
                                light_data.Add($"#pos {asLight!.Header.Translation.X} {asLight!.Header.Translation.Y} {asLight!.Header.Translation.Z}");
                                light_data.Add($"#UNKNOWNFLAGS 0x{asLight!.Header.UnknownFlag1:X8} 0x{asLight!.Header.UnknownFlag2:X8} 0x{asLight!.Header.UnknownFlag3:X8} 0x{asLight!.Header.UnknownFlag4:X8}");
                                light_data.Add($"#UNKNOWN {asLight!.Header.Rotation.X} {asLight!.Header.Rotation.Y} {asLight!.Header.Rotation.Z}");
                                light_data.Add($"#UNKNOWN2 {asLight!.Header.Scale.X} {asLight!.Header.Scale.Y} {asLight!.Header.Scale.Z}");
                                light_data.Add($"#unk {asLight!.Header.Entry1.X} {asLight!.Header.Entry1.Y}");
                                light_data.Add($"#unk2 {asLight!.Header.Entry2.X} {asLight!.Header.Entry2.Y}");
                                light_data.Add($"#unk3 {asLight!.Header.Entry3.X} {asLight!.Header.Entry3.Y}");
                                light_data.Add($"#unk4 {asLight!.Header.Entry4.X} {asLight!.Header.Entry4.Y}");
                                light_data.Add("");
                                break;
                        }
                    }
                    System.IO.File.AppendAllLines(lightsFileName, light_data);
                    light_data.Clear();
                }
            }

            System.IO.File.AppendAllLines(territoryFileName, vertex_data);
            vertex_data.Clear();

            System.IO.File.AppendAllLines(lightsFileName, light_data);
            light_data.Clear();

            Console.WriteLine("\t Finished!");
        }
    }
}

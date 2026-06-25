using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
namespace PlIntersect
{
    public class PlIntersectClass
    {
        [CommandMethod("PLINTERSECT")]
        public void CPlIntersect()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if(doc == null) return;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptEntityOptions cpl1 = new PromptEntityOptions("\nSelect the first closed polyline: ");
            cpl1.SetRejectMessage("\nSelected entity is not a closed polyline.");
            cpl1.AddAllowedClass(typeof(Polyline), true);
            PromptEntityResult cpl1Result = ed.GetEntity(cpl1);
            if (cpl1Result.Status != PromptStatus.OK) return;
            ed.SetImpliedSelection(new ObjectId[] { cpl1Result.ObjectId });
            ed.Regen();
            
            PromptEntityOptions cpl2 = new PromptEntityOptions("\nSelect the second closed polyline: ");
            cpl2.SetRejectMessage("\nSelected entity is not a closed polyline.");
            cpl2.AddAllowedClass(typeof(Polyline), true);
            PromptEntityResult cpl2Result = ed.GetEntity(cpl2);
            if(cpl2Result.Status != PromptStatus.OK) return;
            ed.SetImpliedSelection(new ObjectId[] { cpl1Result.ObjectId, cpl2Result.ObjectId });
            ed.Regen();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Polyline cpoly1 = tr.GetObject(cpl1Result.ObjectId, OpenMode.ForRead) as Polyline;
                Polyline cpoly2 = tr.GetObject(cpl2Result.ObjectId, OpenMode.ForRead) as Polyline;
                cpoly2.Highlight();
                cpoly2.Highlight();

                if (cpoly1 == null || cpoly2 == null)
                {
                    ed.WriteMessage("\nOne or both selected entities are not valid closed polylines.");
                    return;
                }
                
                if(!cpoly1.Closed)
                {
                    if (!cpoly2.Closed)
                    {
                        ed.WriteMessage("\nBoth selected polylines are not closed.");
                        return;
                    }
                    ed.WriteMessage("\nThe first selected polyline is not closed.");
                    return;
                }
                if(!cpoly2.Closed)
                {
                    ed.WriteMessage("\nThe second selected polyline is not closed.");
                    return;
                }

                DBObjectCollection curves1 = new DBObjectCollection();
                curves1.Add(cpoly1);

                DBObjectCollection curves2 = new DBObjectCollection();
                curves2.Add(cpoly2);

                DBObjectCollection cpl1Region = Region.CreateFromCurves(curves1);
                DBObjectCollection cpl2Region = Region.CreateFromCurves(curves2);

                if(cpl1Region.Count == 0 || cpl2Region.Count == 0)
                {
                    ed.WriteMessage("\nFailed to create regions from the selected polylines.");
                    return;
                }

                Region reg1 = cpl1Region[0] as Region;
                Region reg2 = cpl2Region[0] as Region;

                try
                {
                    reg1.BooleanOperation(BooleanOperationType.BoolIntersect, reg2);
                    if(reg1.Area <= 0 )
                    {
                        ed.WriteMessage("\nNo Intersection found.");
                        reg1.Dispose();
                        reg2.Dispose();
                        return;
                    }
                    int[] colorsToAvoid = new int[] { cpoly1.ColorIndex, cpoly2.ColorIndex };
                    int[] priorityColorSeries = new int[] { 1, 3, 5 };

                    foreach(int colorIndex in priorityColorSeries)
                    {
                        if(!colorsToAvoid.Contains(colorIndex))
                        {
                            reg1.ColorIndex = colorIndex;
                            break;
                        }
                    }
                    
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    btr.AppendEntity(reg1);
                    tr.AddNewlyCreatedDBObject(reg1, true);

                    reg2.Dispose();

                    tr.Commit();
                    ed.SetImpliedSelection(new ObjectId[] { reg1.ObjectId });

                    ed.WriteMessage($"\nIntersection region created with area: {reg1.Area}");
                }
                catch (System.Exception ex) {
                    reg1.Dispose();
                    reg2.Dispose();
                    
                    ed.WriteMessage($"\nError during intersection operation: {ex.Message}");
                }
            }
        }
    }

}

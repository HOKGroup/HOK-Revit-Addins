		public class WallPickFilter : ISelectionFilter
		{
			public bool AllowElement(Element e)
			{
				return (e.Category.Id.IntegerValue.Equals(
					(int)BuiltInCategory.OST_Walls));
			}

			public bool AllowReference(Reference r, XYZ p)
			{
				return false;
			}
		}
		
		public void WallJoinsMiterByPick()
		{
			UIDocument uidoc = this.ActiveUIDocument;
			Document doc = uidoc.Document;
			ISelectionFilter wallfilter=new WallPickFilter();
			IList<Reference> refList = new List<Reference>();
			try
			{
				// create a loop to repeatedly prompt the user to select a wall
				while (true)
					refList.Add(uidoc.Selection.PickObject(ObjectType.Element,wallfilter, 
					                                       "Select elements in order to change join type to miter. ESC when finished."));
			}
			// When the user hits ESC Revit will throw an OperationCanceledException which will get them out of the while loop
			catch
			{ }
			using(Transaction t = new Transaction(doc, "Wall joins to Miter"))
			{
				t.Start();
				foreach (Reference r in refList)
				{
					Element wall1= doc.GetElement(r);
					LocationCurve wCurve = wall1.Location as LocationCurve;
					wCurve.set_JoinType(0,JoinType.Miter);
					wCurve.set_JoinType(1,JoinType.Miter);
				}
				doc.Regenerate();
				t.Commit();
			}
		}
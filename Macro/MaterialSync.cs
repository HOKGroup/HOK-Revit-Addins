/*
 * Created by SharpDevelop.
 * User: Jinsol.kim
 * Date: 4/8/2015
 * Time: 10:37 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Excel=Microsoft.Office.Interop.Excel;

namespace MaterialSync
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.DB.Macros.AddInId("ED8E6969-31D9-44CB-8621-3164495A4935")]
	public partial class ThisApplication
	{
		private Document m_doc;
		private List<MaterialFromExcel> materialsFromExcel = new List<MaterialFromExcel>();
		private List<RevitMaterial> revitMaterials = new List<RevitMaterial>();
		
		private void Module_Startup(object sender, EventArgs e)
		{

		}

		private void Module_Shutdown(object sender, EventArgs e)
		{

		}

		#region Revit Macros generated code
		private void InternalStartup()
		{
			this.Startup += new System.EventHandler(Module_Startup);
			this.Shutdown += new System.EventHandler(Module_Shutdown);
		}
		#endregion
		
		public void SyncMaterialList()
		{
			try{
				m_doc=this.ActiveUIDocument.Document;
				
				OpenFileDialog openFile= new OpenFileDialog();
				openFile.Filter = "Excel Files (*.xlsx)|*.xlsx";
				openFile.RestoreDirectory=true;
				
				if(openFile.ShowDialog() == DialogResult.OK)
				{
					string fileName = openFile.FileName;
					if(File.Exists(fileName))
					{
						materialsFromExcel = ReadExcel(fileName);
						if(materialsFromExcel.Count>0)
						{
							revitMaterials=CollectRevitMaterials();
							if(revitMaterials.Count>0)
							{
								if(CompareMaterialSets())
								{
									var materialsToDelete = from mRevit in revitMaterials where mRevit.MatchValue == MatchStatus.ToBeDeleted select mRevit;
									var materialsToAdd=from mExcel in materialsFromExcel where mExcel.MatchValue ==MatchStatus.ToBeAdded select mExcel;
									var materialsToUpdate = from mRevit in revitMaterials where mRevit.MatchValue == MatchStatus.ToBeUpdated select mRevit;
									
									int addCount=materialsToAdd.Count();
									int deleteCount=materialsToDelete.Count();
									int updateCount = materialsToUpdate.Count();
									
									if( addCount > 0 || deleteCount > 0 || updateCount > 0)
									{
										string startupMessage = "Materials to be deleted from the current Revit project: "+deleteCount;
										startupMessage += "\nMaterials to be created from Excel: "+addCount;
										startupMessage +="\nMaterials to be updated in Revit project: "+updateCount;
										
										DialogResult dr=MessageBox.Show(startupMessage+"\n\nWould you like to continue?","Material Sync", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
										if(dr == DialogResult.OK)
										{
											List<ElementId> deletedMaterials = new List<ElementId>();
											List<ElementId> createdMaterials = new List<ElementId>();
											List<ElementId> updatedMaterials = new List<ElementId>();
											
											bool deleted = RemoveMaterials(materialsToDelete.ToList(), out deletedMaterials);
											bool created = CreateMaterials(materialsToAdd.ToList(), out createdMaterials);
											bool updated = UpdateMaterials(materialsToUpdate.ToList(), out updatedMaterials);
											
											string statusMessage = "Deleted Materials: "+ deletedMaterials.Count();
											statusMessage += "\nCreated Materials: "+createdMaterials.Count();
											statusMessage +="\nUpdated Materials: "+updatedMaterials.Count();
										
											
											if(deleted && created && updated)
											{
												MessageBox.Show("All materials are successfully synchronized.\n"+statusMessage, "Material Synchronization", MessageBoxButtons.OK, MessageBoxIcon.Information);
											}
											else{
												MessageBox.Show(statusMessage, "Material Synchronization", MessageBoxButtons.OK, MessageBoxIcon.Information);
											}
										}
									}
									else{
										MessageBox.Show("All existing materials are up to date.","Materials", MessageBoxButtons.OK, MessageBoxIcon.Information);
									}
								}
							}
						}
						else{
							MessageBox.Show("Material information cannot be found from the Excel file.\n"+fileName,"Material Information Missing", MessageBoxButtons.OK , MessageBoxIcon.Information);
						}
					}
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show("Failed to synchronize material list.\n"+ex.Message,"Synchronize Materials", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}
		
		private List<MaterialFromExcel> ReadExcel(string filename)
		{
			List<MaterialFromExcel> materialsInExcel = new List<MaterialFromExcel>();
			try{
				
				Excel.ApplicationClass excel = new Excel.ApplicationClass();
				excel.Visible=false;
				
				Excel.Workbook workbook = excel.Workbooks.Open(filename, 0, true, 5,"","", true, Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
				
				Excel.Worksheet worksheet= (Excel.Worksheet) workbook.Worksheets.get_Item(1);
				
				Excel.Range range = worksheet.UsedRange;
				
				int indexRow = 1;
				while((string)(range.Cells[indexRow, 1] as Excel.Range).Value2 != "FF&E Number")
				{
					indexRow++;
				}
				
				indexRow++;

				while(null!=(range.Cells[indexRow, 1] as Excel.Range).Value2)
				{
					MaterialFromExcel material = new MaterialFromExcel();
					if(null!=(range.Cells[indexRow, 1] as Excel.Range))
					{
						material.FFENumber =(string)(range.Cells[indexRow, 1] as Excel.Range).Value2;
					}
					if(null!=(range.Cells[indexRow, 2] as Excel.Range))
					{
						material.ReferenceValue = (string)(range.Cells[indexRow,2] as Excel.Range).Value2;
					}
					if(null!=(range.Cells[indexRow, 3] as Excel.Range))
					{
						material.NameValue = (string)(range.Cells[indexRow, 3] as Excel.Range).Value2;
					}
					if(null!=(range.Cells[indexRow, 4] as Excel.Range))
					{
						material.SubGroup = (string)(range.Cells[indexRow, 4] as Excel.Range).Value2;
					}
					
					materialsInExcel.Add(material);
					indexRow++;
				}
				
				workbook.Close(false, null, null);
				excel.Quit();

			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return materialsInExcel;
		}
		
		private List<RevitMaterial> CollectRevitMaterials()
		{
			List<RevitMaterial> revitMaterialList=new List<RevitMaterial>();
			try{
				FilteredElementCollector collector = new FilteredElementCollector(m_doc);
				List<Material> materialList = collector.OfClass(typeof(Material)).ToElements().Cast<Material>().ToList();
				if(materialList.Count>0)
				{
					foreach(Material mat in materialList)
					{
						RevitMaterial rMaterial= new RevitMaterial();
						rMaterial.MaterialName = mat.Name;
						rMaterial.MaterialId = mat.Id;
						rMaterial.MaterialCategory=mat.MaterialCategory;
						rMaterial.MaterialClass = mat.MaterialClass;
						
						Parameter param = mat.LookupParameter("Mark");
						if(null!=param)
						{
							if(param.HasValue)
							{
								rMaterial.MarkValue = param.AsString();
							}
						}
						param = mat.LookupParameter("Description");
						if(null!=param)
						{
							if(param.HasValue)
							{
								rMaterial.Description = param.AsString();
							}
						}
						param = mat.LookupParameter("Keynote");
						if(null!=param)
						{
							if(param.HasValue)
							{
								rMaterial.KeynoteValue = param.AsString();
							}
						}
						
						revitMaterialList.Add(rMaterial);
					}
				}
			
			}
			catch(Exception ex)
			{
				MessageBox.Show("Failed to collect materials in the Revit project.\n"+ex.Message,"Collect Revit Materials", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			return revitMaterialList;
		}
		
		private bool CompareMaterialSets()
		{
			bool compared=false;
			try{
				for(int i= materialsFromExcel.Count -1 ; i >-1 ;i--)
				{
					MaterialFromExcel matExcel = materialsFromExcel[i];
					int index =revitMaterials.FindIndex( x => x.MarkValue == matExcel.ReferenceValue);
					if(index >-1)
					{
						RevitMaterial rMaterial = revitMaterials[index];
						rMaterial.MatchedMaterial = matExcel;
						if(rMaterial.MaterialName == matExcel.ReferenceValue &&rMaterial.Description == matExcel.NameValue  && rMaterial.KeynoteValue == matExcel.SubGroup)
						{
							rMaterial.MatchValue = MatchStatus.Matched;
						}
						else{
							rMaterial.MatchValue = MatchStatus.ToBeUpdated;
						}
			
						revitMaterials.RemoveAt(index);
						revitMaterials.Add(rMaterial);
						
						matExcel.MatchedMaterial=rMaterial;
						matExcel.MatchValue=MatchStatus.Matched;
						materialsFromExcel.RemoveAt(i);
						materialsFromExcel.Add(matExcel);
					}
				}
				compared=true;
			}
			catch(Exception ex)
			{
				compared=false;
				MessageBox.Show("Failed to compare materials sets between Excel and Revit project.\n"+ex.Message,"Compare Material Sets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			
			}
			return compared;
		}
		
		private bool RemoveMaterials(List<RevitMaterial> materialsToDelete, out List<ElementId> deletedElementIds)
		{
			deletedElementIds=new List<ElementId>();
			bool removed=false;
			try{
				using(TransactionGroup tg = new TransactionGroup(m_doc))
				{
					tg.Start("Delete Materials");
					foreach(RevitMaterial rm in materialsToDelete)
					{
						using(Transaction trans= new Transaction(m_doc))
						{
							trans.Start("Delete Material");
							try{
								m_doc.Delete(rm.MaterialId);
								deletedElementIds.Add(rm.MaterialId);
								trans.Commit();
							}
							catch(Exception ex)
							{
								trans.RollBack();
								string message=ex.Message;
							}
						}
					}
					tg.Assimilate();
				}
				removed=true;
			}
			catch(Exception ex)
			{
				removed=false;
				MessageBox.Show("Failed to delete materials.\n"+ex.Message, "Delete Materials", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			return removed;
		}
		
		private bool CreateMaterials(List<MaterialFromExcel> materialstoCreate, out List<ElementId> createdMaterialIds)
		{
			createdMaterialIds = new List<ElementId>();
			bool created=false;
			try{
				using(TransactionGroup tg=new TransactionGroup(m_doc))
				{
					tg.Start("Create Materials");
					foreach(MaterialFromExcel mExcel in materialstoCreate)
					{
						if(Material.IsNameUnique(m_doc, mExcel.ReferenceValue))
						{
							using(Transaction trans = new Transaction(m_doc))
							{
								trans.Start("Create Material");
								try{
									//ElementId materialId = Material.Create(m_doc, mExcel.NameValue);
									ElementId materialId = Material.Create(m_doc, mExcel.ReferenceValue);
									Material createdMaterial = m_doc.GetElement(materialId) as Material;
									if(null!=createdMaterial)
									{
										Parameter param = createdMaterial.LookupParameter("Mark");
										if(null!=param)
										{
											param.Set(mExcel.ReferenceValue);
										}
										
										param = createdMaterial.LookupParameter("Description");
										if(null!= param)
										{
											param.Set(mExcel.NameValue);
										}
										
										param = createdMaterial.LookupParameter("Keynote");
										if(null!=param)
										{
											param.Set(mExcel.SubGroup);
										}
									}
									
									createdMaterialIds.Add(materialId);
									trans.Commit();
								}
								catch(Exception ex)
								{
									string message=ex.Message;
									trans.RollBack();
								}
							}
						}
					}

					tg.Assimilate();
				}
				created=true;
			}
			catch(Exception ex)
			{
				created=false;
				MessageBox.Show("Failed to create materials.\n"+ex.Message, "Create Materials", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			return created;
		}
		
		private bool UpdateMaterials(List<RevitMaterial> materialsToUpdate, out List<ElementId> updatedMaterialIds)
		{
			bool updated;
			updatedMaterialIds = new List<ElementId>();
			try{
				using(TransactionGroup tg = new TransactionGroup(m_doc))
				{
					tg.Start("Update Materials");
					foreach(RevitMaterial rm in materialsToUpdate)
					{
						using(Transaction trans = new Transaction(m_doc))
						{
							trans.Start("Update Material");
							try{
								
								Material material = m_doc.GetElement(rm.MaterialId) as Material;
								if(null!=material)
								{
									MaterialFromExcel mExcel = rm.MatchedMaterial;
									//update name
									if(rm.MaterialName != mExcel.ReferenceValue)
									{
										if(Material.IsNameUnique(m_doc,mExcel.ReferenceValue))
										{
											//material.Name = mExcel.NameValue;
											material.Name = mExcel.ReferenceValue;
										}
									}
									//update description
									if(rm.Description!= mExcel.NameValue)
									{
										Parameter param = material.LookupParameter("Description");
										if(null!= param)
										{
											param.Set(mExcel.NameValue);
										}
									}
									//update keynote
									if(rm.KeynoteValue != mExcel.SubGroup)
									{
										Parameter param = material.LookupParameter("Keynote");
										if(null!=param)
										{
											param.Set(mExcel.SubGroup);
										}
									}
									updatedMaterialIds.Add(material.Id);
								}
								trans.Commit();
							}
							catch(Exception ex)
							{
								string message = ex.Message;
								trans.RollBack();
							}
						}
					}
					tg.Assimilate();
				}
				updated=true;
			}
			catch(Exception ex)
			{
				string message = ex.Message;
				updated= false;
			}
			return updated;
		}
				
	}
	
	public enum MatchStatus{ Matched, ToBeDeleted, ToBeAdded, ToBeUpdated }
	public class MaterialFromExcel
	{
		private string ffeNumber = "";
		private string referenceValue = ""; //map to mark and name
		private string nameValue=""; //map to description
		private string subGroup="";
		private MatchStatus matchValue=MatchStatus.ToBeAdded;
		private RevitMaterial matchedMaterial = null;
	
		public string FFENumber { get { return ffeNumber; } set { ffeNumber=value; } }
		public string ReferenceValue {get{return referenceValue;} set { referenceValue = value;} }
		public string NameValue {get{return nameValue;}set{nameValue=value;}}
		public string SubGroup{get{return subGroup ;}set{subGroup = value;}}
		public MatchStatus MatchValue {get{return matchValue;}set{matchValue=value;}}
		public RevitMaterial MatchedMaterial {get{return matchedMaterial;}set{matchedMaterial =value;}}
		
		public MaterialFromExcel()
		{
		
		}
	
	}
	
	public class RevitMaterial
	{
		private string materialName="";
		private ElementId materialId=ElementId.InvalidElementId;
		private string markValue="";
		private string keynoteValue = "";
		private string materialClass="";
		private string materialCategory="";
		private string description = "";
		private MatchStatus matchValue=MatchStatus.ToBeDeleted;
		private MaterialFromExcel matchedMaterial = null;
		
		public string MaterialName{get{return materialName;}set{materialName=value;}}
		public ElementId MaterialId{get{return materialId;}set{materialId=value;}}
		public string MarkValue{get{return markValue;}set{markValue=value;}}
		public string KeynoteValue {get{return keynoteValue;}set{keynoteValue=value;}}
		public string MaterialClass{get{return materialClass;}set{materialClass=value;}}
		public string MaterialCategory {get{return materialCategory;}set{materialCategory=value;}}
		public string Description{ get {return description;}set{description=value;}}
		public MatchStatus MatchValue {get{return matchValue;}set{matchValue=value;}}
		public MaterialFromExcel MatchedMaterial {get{return matchedMaterial;}set{matchedMaterial = value;}}
		
		public RevitMaterial()
		{
			
		}
	}
}
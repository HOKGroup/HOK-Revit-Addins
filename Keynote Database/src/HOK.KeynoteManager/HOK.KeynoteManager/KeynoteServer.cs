using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.KeynoteManager
{
    public class KeynoteServer:IExternalResourceServer
    {

        private string versionNumber = "";
        
        public KeynoteServer(ControlledApplication app)
        {
            versionNumber = app.VersionNumber;
        }


        public bool AreSameResources(IDictionary<string, string> reference1, IDictionary<string, string> reference2)
        {
            bool same = true;
            try
            {
                if (reference1.Count != reference2.Count)
                {
                    same = false;
                }
                else
                {
                    foreach (string key in reference1.Keys)
                    {
                        if (!reference2.ContainsKey(key) || reference1[key] != reference2[key])
                        {
                            same = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return same;
        }

        public string GetIconPath()
        {
            return string.Empty;
        }

        public string GetInSessionPath(ExternalResourceReference reference, string originalDisplayPath)
        {
            return originalDisplayPath;
        }

        public string GetInformationLink()
        {
            return "http://www.hok.com/";
        }

        public ResourceVersionStatus GetResourceVersionStatus(ExternalResourceReference reference)
        {
            throw new NotImplementedException();
        }

        public string GetShortName()
        {
            return GetName();
        }

        public void GetTypeSpecificServerOperations(ExternalResourceServerExtensions extensions)
        {
            throw new NotImplementedException();
        }

        public bool IsResourceWellFormed(ExternalResourceReference extRef)
        {
            bool valid = false;
            try
            {
                if (extRef.ServerId != GetServerId()) { return false; }
                if (!extRef.HasValidDisplayPath()) { return false; }

                IDictionary<string, string> refMap = extRef.GetReferenceInformation();
                if (refMap.ContainsKey("VersionNumber"))
                {
                    valid = KeynoteDatabase.IsValidRevitVersion(refMap["VersionNumber"]);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return valid;
        }

        public void LoadResource(Guid loadRequestId, ExternalResourceType resourceType, ExternalResourceReference desiredResource, ExternalResourceLoadContext loadContext, ExternalResourceLoadContent loadResults)
        {
            try
            {
                loadResults.LoadStatus = ExternalResourceLoadStatus.Failure;

                if (loadRequestId == null)
                    throw new ArgumentNullException("loadRequestId"); ;
                if (resourceType == null)
                    throw new ArgumentNullException("resourceType"); ;
                if (desiredResource == null)
                    throw new ArgumentNullException("resourceReference"); ;
                if (loadContext == null)
                    throw new ArgumentNullException("loadContext"); ;
                if (loadResults == null)
                    throw new ArgumentNullException("loadContent"); ;
                if (!SupportsExternalResourceType(resourceType))
                    throw new ArgumentOutOfRangeException("resourceType", "The specified resource type is not supported by this server.");

                loadResults.Version = GetCurrentAvailableResourceVersion(desiredResource);

                if (resourceType == ExternalResourceTypes.BuiltInExternalResourceTypes.KeynoteTable)
                {
                    LoadKeynoteDataResource(desiredResource, loadResults);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private string GetCurrentAvailableResourceVersion(ExternalResourceReference extResRef)
        {
            string databaseVersion = "";
            try
            {
                IDictionary<string, string> refMap = extResRef.GetReferenceInformation();
                if (refMap.ContainsKey("VersionNumber"))
                {
                    return KeynoteDatabase.GetCurrentVersion();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return databaseVersion;
        }

        private void LoadKeynoteDataResource(ExternalResourceReference resourceReference, ExternalResourceLoadContent loadContent)
        {
            try
            {
                KeyBasedTreeEntriesLoadContent entriesContent = (KeyBasedTreeEntriesLoadContent)loadContent;
                if (null == entriesContent)
                {
                    throw new ArgumentException("Wrong type of ExternalResourceLoadContent (expecting a KeyBasedTreeEntriesLoadContent) for keynote data.", "loadContent");
                }

                entriesContent.Reset();
                IDictionary<string, string> refMap = resourceReference.GetReferenceInformation();
                if (refMap.ContainsKey("VersionNumber") && refMap.ContainsKey("ProjectName"))
                {
                    KeynoteDatabase.LoadKeynoteEntries(refMap, ref entriesContent);
                    entriesContent.BuildEntries();
                    loadContent.LoadStatus = ExternalResourceLoadStatus.Success;
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void SetupBrowserData(ExternalResourceBrowserData browseData)
        {
            try
            {
                ExternalResourceMatchOptions options = browseData.GetMatchOptions();
                ExternalResourceType resourceType = options.ResourceType;

                if (resourceType == ExternalResourceTypes.BuiltInExternalResourceTypes.KeynoteTable)
                {
                    SetupKeynoteDatabaseBrowserData(browseData);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void SetupKeynoteDatabaseBrowserData(ExternalResourceBrowserData browserData)
        {
            try
            {
                string folderPath = browserData.FolderPath;
                string versionFolder = "RVT" + versionNumber;
                if (folderPath == "/")
                {
                    browserData.AddSubFolder("RVT2014");
                    browserData.AddSubFolder("RVT2015");
                    browserData.AddSubFolder("RVT2016");
                }
                else if (folderPath.EndsWith("/" + versionFolder))
                {
                    Dictionary<string, string> referenceMap = new Dictionary<string, string>();
                    referenceMap.Add("VersionNumber", versionNumber);
                    referenceMap.Add("ProjectName", "Test");
                    browserData.AddResource("Test Keynote.txt", KeynoteDatabase.GetCurrentVersion(), referenceMap);

                    Dictionary<string, string> referenceMap2 = new Dictionary<string, string>();
                    referenceMap2.Add("VersionNumber", versionNumber);
                    referenceMap2.Add("ProjectName", "LocalDB");
                    browserData.AddResource("KeynoteDB_local.txt", KeynoteDatabase.GetCurrentVersion(), referenceMap2);

                    Dictionary<string, string> referenceMap3 = new Dictionary<string, string>();
                    referenceMap3.Add("VersionNumber", versionNumber);
                    referenceMap3.Add("ProjectName", "CloudDB");
                    browserData.AddResource("KeynoteDB_cloud.txt", KeynoteDatabase.GetCurrentVersion(), referenceMap3);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            } 
        }


        public bool SupportsExternalResourceType(ExternalResourceType type)
        {
            return (type == ExternalResourceTypes.BuiltInExternalResourceTypes.KeynoteTable);
        }

        public string GetDescription()
        {
            return "An external resource server which provides keynote data";
        }

        public string GetName()
        {
            return "HOK Keynote Server";
        }

        public Guid GetServerId()
        {
            return new Guid("54DC3505-F6EB-4558-8252-1B16A9F8135C");
        }

        public ExternalServiceId GetServiceId()
        {
            return ExternalServices.BuiltInExternalServices.ExternalResourceService;
        }

        public string GetVendorId()
        {
            return "HOK";
        }
    }
}

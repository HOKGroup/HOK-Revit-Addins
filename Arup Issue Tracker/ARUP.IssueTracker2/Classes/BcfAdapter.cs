using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;

namespace ARUP.IssueTracker.Classes
{
    // This is a class to convert data between BCF 1.0 and 2.0 
    public static class BcfAdapter
    {
        /// <summary>
        /// Load from BCF 1.0 to BCF 2.0
        /// </summary>
        /// <param name="bcf1">BCF 1.0 file</param>
        public static ARUP.IssueTracker.Classes.BCF2.Markup LoadBcf2IssueFromBcf1(ARUP.IssueTracker.Classes.BCF1.IssueBCF bcf1Issue)
        {
            try
            {
                // Set up a snapshot/viewpoint guid for Solibri
                string snapshotGuid = Guid.NewGuid().ToString();

                // Convert header files
                List<BCF2.HeaderFile> bcf2Headers = new List<BCF2.HeaderFile>();
                if (bcf1Issue.markup.Header != null)
                {
                    foreach (BCF1.HeaderFile bcf1Header in bcf1Issue.markup.Header)
                    {
                        bcf2Headers.Add(new BCF2.HeaderFile()
                        {
                            Date = DateTime.Now,
                            DateSpecified = true,
                            Filename = bcf1Header.Filename,
                            IfcProject = bcf1Header.IfcProject,
                            IfcSpatialStructureElement = bcf1Header.IfcSpatialStructureElement,
                            isExternal = true, // default true for now
                            Reference = "" // default empty for now
                        });
                    }
                }

                // Convert Comments
                ObservableCollection<BCF2.Comment> bcf2Comments = new ObservableCollection<BCF2.Comment>();
                if (bcf1Issue.markup.Comment != null)
                {
                    foreach (BCF1.CommentBCF bcf1Comment in bcf1Issue.markup.Comment)
                    {
                        if (bcf1Comment != null)
                        {
                            bcf2Comments.Add(new BCF2.Comment()
                            {
                                Author = bcf1Comment.Author,
                                Comment1 = bcf1Comment.Comment1,
                                Date = bcf1Comment.Date,
                                Guid = bcf1Comment.Guid,
                                ModifiedAuthor = bcf1Comment.Author,  // default the same as author for now
                                ModifiedDate = DateTime.Now,  // set to now for Solibri
                                ModifiedDateSpecified = true,
                                ReplyToComment = null, // mismatch attribute
                                Status = bcf1Comment.Status.ToString(),
                                Topic = new BCF2.CommentTopic() { Guid = bcf1Issue.markup.Topic.Guid }, // all referenced to markup's topic
                                VerbalStatus = bcf1Comment.VerbalStatus
                            });
                        }
                    }
                }

                // Convert Topic
                BCF2.Topic bcf2Topic = new BCF2.Topic()
                {
                    AssignedTo = null,  // mismatch attribute
                    BimSnippet = null,  // mismatch attribute
                    CreationAuthor = null,  // mismatch attribute
                    CreationDate = DateTime.Now,  // set to now for Solibri
                    CreationDateSpecified = true,
                    Description = null,  // mismatch attribute
                    DocumentReferences = null,  // mismatch attribute
                    Guid = bcf1Issue.markup.Topic.Guid,
                    Index = null,  // mismatch attribute
                    Labels = null,  // mismatch attribute
                    ModifiedAuthor = null,  // mismatch attribute
                    ModifiedDate = DateTime.Now,  // set to now for Solibri
                    ModifiedDateSpecified = true,
                    Priority = null,  // mismatch attribute
                    ReferenceLink = bcf1Issue.markup.Topic.ReferenceLink,
                    RelatedTopics = null,  // mismatch attribute
                    Title = bcf1Issue.markup.Topic.Title,
                    TopicStatus = null,  // mismatch attribute
                    TopicType = null  // mismatch attribute
                };

                // Convert ClippingPlane
                List<BCF2.ClippingPlane> bcf2ClippingPlanes = new List<BCF2.ClippingPlane>();
                if (bcf1Issue.viewpoint.ClippingPlanes != null)
                {
                    foreach (BCF1.ClippingPlane bcf1ClippingPlane in bcf1Issue.viewpoint.ClippingPlanes)
                    {
                        if (bcf1ClippingPlane != null)
                        {
                            bcf2ClippingPlanes.Add(new BCF2.ClippingPlane()
                            {
                                Direction = new BCF2.Direction()
                                {
                                    X = bcf1ClippingPlane.Direction.X,
                                    Y = bcf1ClippingPlane.Direction.Y,
                                    Z = bcf1ClippingPlane.Direction.Z
                                },
                                Location = new BCF2.Point()
                                {
                                    X = bcf1ClippingPlane.Location.X,
                                    Y = bcf1ClippingPlane.Location.Y,
                                    Z = bcf1ClippingPlane.Location.Z
                                }
                            });
                        }
                    }
                }

                // Convert Components
                List<BCF2.Component> bcf2Components = new List<BCF2.Component>();
                if (bcf1Issue.viewpoint.Components != null)
                {
                    foreach (BCF1.Component bcf1Component in bcf1Issue.viewpoint.Components)
                    {
                        if (bcf1Component != null)
                        {
                            bcf2Components.Add(new BCF2.Component()
                            {
                                AuthoringToolId = bcf1Component.AuthoringToolId,
                                // Color = bcf1Component,    // mismatch attribute
                                IfcGuid = string.IsNullOrWhiteSpace(bcf1Component.IfcGuid) ? IfcGuid.ToIfcGuid(Guid.NewGuid()) : bcf1Component.IfcGuid,
                                OriginatingSystem = bcf1Component.OriginatingSystem
                                // Selected = bcf1Component,   // mismatch attribute
                                // Visible = bcf1Component    // mismatch attribute
                            });
                        }
                    }
                }

                // Convert Lines
                List<BCF2.Line> bcf2Lines = new List<BCF2.Line>();
                if (bcf1Issue.viewpoint.Lines != null)
                {
                    foreach (BCF1.Line bcf1Line in bcf1Issue.viewpoint.Lines)
                    {
                        if (bcf1Line != null)
                        {
                            bcf2Lines.Add(new BCF2.Line()
                            {
                                StartPoint = new BCF2.Point()
                                {
                                    X = bcf1Line.StartPoint.X,
                                    Y = bcf1Line.StartPoint.Y,
                                    Z = bcf1Line.StartPoint.Z
                                },
                                EndPoint = new BCF2.Point()
                                {
                                    X = bcf1Line.EndPoint.X,
                                    Y = bcf1Line.EndPoint.Y,
                                    Z = bcf1Line.EndPoint.Z
                                }
                            });
                        }
                    }
                }

                // Convert VisualizationInfo
                BCF2.VisualizationInfo bcf2VizInfo = new BCF2.VisualizationInfo()
                {
                    Bitmaps = null, // default null 
                    ClippingPlanes = bcf2ClippingPlanes.ToArray(),
                    Components = bcf2Components,
                    Lines = bcf2Lines.ToArray(),
                    OrthogonalCamera = bcf1Issue.viewpoint.OrthogonalCamera == null ? null : new BCF2.OrthogonalCamera()
                    {
                        CameraDirection = new BCF2.Direction()
                        {
                            X = bcf1Issue.viewpoint.OrthogonalCamera.CameraDirection.X,
                            Y = bcf1Issue.viewpoint.OrthogonalCamera.CameraDirection.Y,
                            Z = bcf1Issue.viewpoint.OrthogonalCamera.CameraDirection.Z
                        },
                        CameraUpVector = new BCF2.Direction()
                        {
                            X = bcf1Issue.viewpoint.OrthogonalCamera.CameraUpVector.X,
                            Y = bcf1Issue.viewpoint.OrthogonalCamera.CameraUpVector.Y,
                            Z = bcf1Issue.viewpoint.OrthogonalCamera.CameraUpVector.Z
                        },
                        CameraViewPoint = new BCF2.Point()
                        {
                            X = bcf1Issue.viewpoint.OrthogonalCamera.CameraViewPoint.X,
                            Y = bcf1Issue.viewpoint.OrthogonalCamera.CameraViewPoint.Y,
                            Z = bcf1Issue.viewpoint.OrthogonalCamera.CameraViewPoint.Z
                        },
                        ViewToWorldScale = bcf1Issue.viewpoint.OrthogonalCamera.ViewToWorldScale
                    },
                    PerspectiveCamera = bcf1Issue.viewpoint.PerspectiveCamera == null ? null : new BCF2.PerspectiveCamera()
                    {
                        CameraDirection = new BCF2.Direction()
                        {
                            X = bcf1Issue.viewpoint.PerspectiveCamera.CameraDirection.X,
                            Y = bcf1Issue.viewpoint.PerspectiveCamera.CameraDirection.Y,
                            Z = bcf1Issue.viewpoint.PerspectiveCamera.CameraDirection.Z
                        },
                        CameraUpVector = new BCF2.Direction()
                        {
                            X = bcf1Issue.viewpoint.PerspectiveCamera.CameraUpVector.X,
                            Y = bcf1Issue.viewpoint.PerspectiveCamera.CameraUpVector.Y,
                            Z = bcf1Issue.viewpoint.PerspectiveCamera.CameraUpVector.Z
                        },
                        CameraViewPoint = new BCF2.Point()
                        {
                            X = bcf1Issue.viewpoint.PerspectiveCamera.CameraViewPoint.X,
                            Y = bcf1Issue.viewpoint.PerspectiveCamera.CameraViewPoint.Y,
                            Z = bcf1Issue.viewpoint.PerspectiveCamera.CameraViewPoint.Z
                        },
                        FieldOfView = bcf1Issue.viewpoint.PerspectiveCamera.FieldOfView
                    },
                    SheetCamera = bcf1Issue.viewpoint.SheetCamera == null ? null : new BCF2.SheetCamera()
                    {
                        SheetID = bcf1Issue.viewpoint.SheetCamera.SheetID,
                        SheetName = null, // default null
                        TopLeft = new BCF2.Point()
                        {
                            X = bcf1Issue.viewpoint.SheetCamera.TopLeft.X,
                            Y = bcf1Issue.viewpoint.SheetCamera.TopLeft.Y,
                            Z = bcf1Issue.viewpoint.SheetCamera.TopLeft.Z
                        },
                        BottomRight = new BCF2.Point()
                        {
                            X = bcf1Issue.viewpoint.SheetCamera.BottomRight.X,
                            Y = bcf1Issue.viewpoint.SheetCamera.BottomRight.Y,
                            Z = bcf1Issue.viewpoint.SheetCamera.BottomRight.Z
                        }
                    }
                };

                // Convert viewpoints
                // BCF 1.0 can only have one viewpoint
                ObservableCollection<BCF2.ViewPoint> bcf2ViewPoints = new ObservableCollection<BCF2.ViewPoint>();
                bcf2ViewPoints.Add(new BCF2.ViewPoint()
                {
                    Guid = snapshotGuid,    // for Solibri
                    Snapshot = Path.GetFileName(bcf1Issue.snapshot),
                    SnapshotPath = bcf1Issue.snapshot,
                    Viewpoint = "viewpoint.bcfv",
                    VisInfo = bcf2VizInfo
                });

                // return BCF 2 issue
                return new BCF2.Markup()
                {
                    Header = bcf2Headers,
                    Comment = bcf2Comments,
                    Topic = bcf2Topic,
                    Viewpoints = bcf2ViewPoints
                };

            }
            catch (Exception ex)
            {
                // Get stack trace for the exception with source file information
                var st = new System.Diagnostics.StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                System.Windows.MessageBox.Show("Exception:" + line + "=====" + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Save BCF 1.0 as BCF 2.0
        /// </summary>
        /// <param name="bcf1">BCF 1.0 file</param>
        public static void SaveBcf2FromBcf1(BCF1.BCF bcf1)
        {
            try
            {
                BCF2.BcfFile bcf2 = new BCF2.BcfFile();
                bcf2.TempPath = bcf1.path; // replace temp path with BCF 1.0's

                // bcf2.ProjectName = ;    // from Jira?
                // bcf2.ProjectId = ;      // from Jira?

                // Add issues (markups)
                foreach (BCF1.IssueBCF bcf1Issue in bcf1.Issues)
                {
                    // Set up a snapshot/viewpoint guid for Solibri
                    string snapshotGuid = Guid.NewGuid().ToString();

                    // Convert header files
                    List<BCF2.HeaderFile> bcf2Headers = new List<BCF2.HeaderFile>();
                    if (bcf1Issue.markup.Header != null)
                    {                        
                        foreach (BCF1.HeaderFile bcf1Header in bcf1Issue.markup.Header)
                        {
                            bcf2Headers.Add(new BCF2.HeaderFile()
                            {
                                Date = DateTime.Now, 
                                DateSpecified = true,
                                Filename = bcf1Header.Filename,
                                IfcProject = bcf1Header.IfcProject,
                                IfcSpatialStructureElement = bcf1Header.IfcSpatialStructureElement,
                                isExternal = true, // default true for now
                                Reference = "" // default empty for now
                            });
                        }
                    }

                    // Convert Comments
                    ObservableCollection<BCF2.Comment> bcf2Comments = new ObservableCollection<BCF2.Comment>();
                    if (bcf1Issue.markup.Comment != null)
                    {
                        foreach (BCF1.CommentBCF bcf1Comment in bcf1Issue.markup.Comment)
                        {
                            if (bcf1Comment != null)
                            {
                                bcf2Comments.Add(new BCF2.Comment()
                                {
                                    Author = bcf1Comment.Author,
                                    Comment1 = bcf1Comment.Comment1,
                                    Date = bcf1Comment.Date,
                                    Guid = bcf1Comment.Guid,
                                    ModifiedAuthor = bcf1Comment.Author,  // default the same as author for now
                                    ModifiedDate = DateTime.Now,  // set to now for Solibri
                                    ModifiedDateSpecified = true,
                                    ReplyToComment = null, // mismatch attribute
                                    Status = bcf1Comment.Status.ToString(),
                                    Topic = new BCF2.CommentTopic() { Guid = bcf1Issue.markup.Topic.Guid }, // all referenced to markup's topic
                                    VerbalStatus = bcf1Comment.VerbalStatus,
                                    Viewpoint = new BCF2.CommentViewpoint() { Guid = snapshotGuid } // for Solibri
                                });
                            }
                        }
                    }                   

                    // Convert Topic
                    BCF2.Topic bcf2Topic = new BCF2.Topic()
                    {
                        AssignedTo = null,  // mismatch attribute
                        BimSnippet = null,  // mismatch attribute
                        CreationAuthor = null,  // mismatch attribute
                        CreationDate = DateTime.Now,  // set to now for Solibri
                        CreationDateSpecified = true,
                        Description = null,  // mismatch attribute
                        DocumentReferences = null,  // mismatch attribute
                        Guid = bcf1Issue.markup.Topic.Guid,
                        Index = null,  // mismatch attribute
                        Labels = null,  // mismatch attribute
                        ModifiedAuthor = null,  // mismatch attribute
                        ModifiedDate = DateTime.Now,  // set to now for Solibri
                        ModifiedDateSpecified = true,
                        Priority = null,  // mismatch attribute
                        ReferenceLink = bcf1Issue.markup.Topic.ReferenceLink,
                        RelatedTopics = null,  // mismatch attribute
                        Title = bcf1Issue.markup.Topic.Title,
                        TopicStatus = null,  // mismatch attribute
                        TopicType = null  // mismatch attribute
                    };

                    // Convert ClippingPlane
                    List<BCF2.ClippingPlane> bcf2ClippingPlanes = new List<BCF2.ClippingPlane>();
                    if (bcf1Issue.viewpoint.ClippingPlanes != null)
                    {
                        foreach (BCF1.ClippingPlane bcf1ClippingPlane in bcf1Issue.viewpoint.ClippingPlanes)
                        {
                            if (bcf1ClippingPlane != null)
                            {
                                bcf2ClippingPlanes.Add(new BCF2.ClippingPlane()
                                {
                                    Direction = new BCF2.Direction()
                                    {
                                        X = bcf1ClippingPlane.Direction.X,
                                        Y = bcf1ClippingPlane.Direction.Y,
                                        Z = bcf1ClippingPlane.Direction.Z
                                    },
                                    Location = new BCF2.Point()
                                    {
                                        X = bcf1ClippingPlane.Location.X,
                                        Y = bcf1ClippingPlane.Location.Y,
                                        Z = bcf1ClippingPlane.Location.Z
                                    }
                                });
                            }
                        }
                    }                    

                    // Convert Components
                    List<BCF2.Component> bcf2Components = new List<BCF2.Component>();
                    if (bcf1Issue.viewpoint.Components != null)
                    {
                        foreach (BCF1.Component bcf1Component in bcf1Issue.viewpoint.Components)
                        {
                            if (bcf1Component != null)
                            {
                                bcf2Components.Add(new BCF2.Component()
                                {
                                    AuthoringToolId = bcf1Component.AuthoringToolId,
                                    // Color = bcf1Component,    // mismatch attribute
                                    IfcGuid = string.IsNullOrWhiteSpace(bcf1Component.IfcGuid) ? IfcGuid.ToIfcGuid(Guid.NewGuid()) : bcf1Component.IfcGuid,
                                    OriginatingSystem = bcf1Component.OriginatingSystem
                                    // Selected = bcf1Component,   // mismatch attribute
                                    // Visible = bcf1Component    // mismatch attribute
                                });
                            }
                        }
                    }                    

                    // Convert Lines
                    List<BCF2.Line> bcf2Lines = new List<BCF2.Line>();
                    if (bcf1Issue.viewpoint.Lines != null)
                    {
                        foreach (BCF1.Line bcf1Line in bcf1Issue.viewpoint.Lines)
                        {
                            if (bcf1Line != null)
                            {
                                bcf2Lines.Add(new BCF2.Line()
                                {
                                    StartPoint = new BCF2.Point()
                                    {
                                        X = bcf1Line.StartPoint.X,
                                        Y = bcf1Line.StartPoint.Y,
                                        Z = bcf1Line.StartPoint.Z
                                    },
                                    EndPoint = new BCF2.Point()
                                    {
                                        X = bcf1Line.EndPoint.X,
                                        Y = bcf1Line.EndPoint.Y,
                                        Z = bcf1Line.EndPoint.Z
                                    }
                                });
                            }
                        }
                    }
                   
                    // Convert VisualizationInfo
                    BCF2.VisualizationInfo bcf2VizInfo = new BCF2.VisualizationInfo()
                    {
                        Bitmaps = null, // default null 
                        ClippingPlanes = bcf2ClippingPlanes.ToArray(),
                        Components = bcf2Components,
                        Lines = bcf2Lines.ToArray(),
                        OrthogonalCamera = bcf1Issue.viewpoint.OrthogonalCamera == null ? null : new BCF2.OrthogonalCamera()
                        {
                            CameraDirection = new BCF2.Direction()
                            {
                                X = bcf1Issue.viewpoint.OrthogonalCamera.CameraDirection.X,
                                Y = bcf1Issue.viewpoint.OrthogonalCamera.CameraDirection.Y,
                                Z = bcf1Issue.viewpoint.OrthogonalCamera.CameraDirection.Z
                            },
                            CameraUpVector = new BCF2.Direction()
                            {
                                X = bcf1Issue.viewpoint.OrthogonalCamera.CameraUpVector.X,
                                Y = bcf1Issue.viewpoint.OrthogonalCamera.CameraUpVector.Y,
                                Z = bcf1Issue.viewpoint.OrthogonalCamera.CameraUpVector.Z
                            },
                            CameraViewPoint = new BCF2.Point()
                            {
                                X = bcf1Issue.viewpoint.OrthogonalCamera.CameraViewPoint.X,
                                Y = bcf1Issue.viewpoint.OrthogonalCamera.CameraViewPoint.Y,
                                Z = bcf1Issue.viewpoint.OrthogonalCamera.CameraViewPoint.Z
                            },
                            ViewToWorldScale = bcf1Issue.viewpoint.OrthogonalCamera.ViewToWorldScale
                        },
                        PerspectiveCamera = bcf1Issue.viewpoint.PerspectiveCamera == null ? null : new BCF2.PerspectiveCamera()
                        {
                            CameraDirection = new BCF2.Direction()
                            {
                                X = bcf1Issue.viewpoint.PerspectiveCamera.CameraDirection.X,
                                Y = bcf1Issue.viewpoint.PerspectiveCamera.CameraDirection.Y,
                                Z = bcf1Issue.viewpoint.PerspectiveCamera.CameraDirection.Z
                            },
                            CameraUpVector = new BCF2.Direction()
                            {
                                X = bcf1Issue.viewpoint.PerspectiveCamera.CameraUpVector.X,
                                Y = bcf1Issue.viewpoint.PerspectiveCamera.CameraUpVector.Y,
                                Z = bcf1Issue.viewpoint.PerspectiveCamera.CameraUpVector.Z
                            },
                            CameraViewPoint = new BCF2.Point()
                            {
                                X = bcf1Issue.viewpoint.PerspectiveCamera.CameraViewPoint.X,
                                Y = bcf1Issue.viewpoint.PerspectiveCamera.CameraViewPoint.Y,
                                Z = bcf1Issue.viewpoint.PerspectiveCamera.CameraViewPoint.Z
                            },
                            FieldOfView = bcf1Issue.viewpoint.PerspectiveCamera.FieldOfView
                        },
                        SheetCamera = bcf1Issue.viewpoint.SheetCamera == null ? null : new BCF2.SheetCamera()
                        {
                            SheetID = bcf1Issue.viewpoint.SheetCamera.SheetID,
                            SheetName = null, // default null
                            TopLeft = new BCF2.Point()
                            {
                                X = bcf1Issue.viewpoint.SheetCamera.TopLeft.X,
                                Y = bcf1Issue.viewpoint.SheetCamera.TopLeft.Y,
                                Z = bcf1Issue.viewpoint.SheetCamera.TopLeft.Z
                            },
                            BottomRight = new BCF2.Point()
                            {
                                X = bcf1Issue.viewpoint.SheetCamera.BottomRight.X,
                                Y = bcf1Issue.viewpoint.SheetCamera.BottomRight.Y,
                                Z = bcf1Issue.viewpoint.SheetCamera.BottomRight.Z
                            }
                        }
                    };

                    // Convert viewpoints
                    // BCF 1.0 can only have one viewpoint
                    ObservableCollection<BCF2.ViewPoint> bcf2ViewPoints = new ObservableCollection<BCF2.ViewPoint>();
                    bcf2ViewPoints.Add(new BCF2.ViewPoint()
                    {
                        Guid = snapshotGuid,    // for Solibri
                        Snapshot = bcf1Issue.snapshot,
                        Viewpoint = "viewpoint.bcfv",
                        VisInfo = bcf2VizInfo
                    });

                    // Add BCF 2.0 issues/markups
                    bcf2.Issues.Add(new BCF2.Markup()
                    {
                        Header = bcf2Headers,
                        Comment = bcf2Comments,
                        Topic = bcf2Topic,
                        Viewpoints = bcf2ViewPoints
                    });
                }

                // Save BCF 2.0 file
                BCF2.BcfContainer.SaveBcfFile(bcf2);
                bcf1.HasBeenSaved = true;
            }
            catch (Exception ex)
            {
                // Get stack trace for the exception with source file information
                var st = new System.Diagnostics.StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                System.Windows.MessageBox.Show("Exception:" + line + "=====" + ex.ToString());
            }            
            
        }

        /// <summary>
        /// Save Jira issues as BCF 2.0
        /// </summary>
        /// <param name="jiraPan"></param>
        public static void SaveBcf2FromJira(UserControls.MainPanel mainPan)
        {
            try
            {
                BCF2.BcfFile bcf2 = new BCF2.BcfFile();
                string ReportFolder = Path.Combine(Path.GetTempPath(), "BCFtemp", Path.GetRandomFileName());
                bcf2.TempPath = ReportFolder;

                if (!Directory.Exists(ReportFolder))
                    Directory.CreateDirectory(ReportFolder);

                bcf2.ProjectName = ((Project)(mainPan.jiraPan.projCombo.SelectedItem)).name;
                //bcf2.ProjectId = ;      // Is there a guid for a Jira project?

                //inject ExtensionSchema.xsd
                List<string> statues = new List<string>();
                mainPan.jiraPan.statusfilter.checkboxes.ForEach(o => statues.Add(o.Content.ToString()));
                List<string> types = mainPan.jira.TypesCollection.Select(o => o.name).ToList();
                List<string> priorities = mainPan.jira.PrioritiesCollection.Select(o => o.name).ToList();

                WriteExtensionSchema(Path.Combine(ReportFolder, "ExtensionSchema.xsd"), types, statues, priorities);

                int errors = 0;

                // Add issues (markups)
                foreach (object t in mainPan.jiraPan.issueList.SelectedItems)
                {                    
                    int index = mainPan.jiraPan.issueList.Items.IndexOf(t);
                    Issue issue = mainPan.jira.IssuesCollection[index];
                    if (issue.viewpoint == "" || issue.snapshotFull == "")
                    {
                        errors++;
                        continue;
                    }

                    // Create temp. folder
                    string issueGuid = issue.fields.guid;
                    if (!Directory.Exists(Path.Combine(ReportFolder, issueGuid)))
                        Directory.CreateDirectory(Path.Combine(ReportFolder, issueGuid));

                    // Set up a default snapshot/viewpoint guid for Solibri
                    string snapshotGuid = Guid.NewGuid().ToString();

                    // Convert header files
                    List<BCF2.HeaderFile> bcf2Headers = new List<BCF2.HeaderFile>();
                    bcf2Headers.Add(new BCF2.HeaderFile()
                    {
                        Date = string.IsNullOrWhiteSpace(issue.fields.created) ? DateTime.Now : DateTime.Parse(issue.fields.created),
                        DateSpecified = true,
                        Filename = "Jira Export " + DateTime.Now.ToShortDateString().Replace("/", "-"),
                        isExternal = true, // default true for now
                        Reference = "",  // default empty for now
                        IfcProject = IfcGuid.ToIfcGuid(Guid.NewGuid())
                    });

                    // Convert viewpoints in description
                    ObservableCollection<BCF2.ViewPoint> bcf2ViewPoints = new ObservableCollection<BCF2.ViewPoint>();
                    bcf2ViewPoints.Add(new BCF2.ViewPoint(true));

                    // Convert Comments 
                    ObservableCollection<BCF2.Comment> bcf2Comments = new ObservableCollection<BCF2.Comment>();
                    foreach (var comm in issue.fields.comment.comments)
                    {
                        if (comm != null)
                        {            
                            BCF2.Comment bcf2Comment = new BCF2.Comment()
                            {
                                Author = comm.author == null ? null : comm.author.displayName,
                                Comment1 = comm.body == null ? null : comm.body,
                                Date = string.IsNullOrWhiteSpace(comm.created) ? DateTime.Now : DateTime.Parse(comm.created),
                                Guid = Guid.NewGuid().ToString(),
                                ModifiedAuthor = comm.updateAuthor == null ? null : comm.updateAuthor.displayName,
                                ModifiedDate = string.IsNullOrWhiteSpace(comm.updated) ? DateTime.Now : DateTime.Parse(comm.updated),
                                ModifiedDateSpecified = true,
                                ReplyToComment = null, // default null
                                Status = "Unknown",
                                Topic = new BCF2.CommentTopic() { Guid = issueGuid }, // all referenced to markup's topic
                                VerbalStatus = issue.fields.status == null ? null : issue.fields.status.name                         
                            };

                            BCF2.ViewPoint vp = new BCF2.ViewPoint(false);
                            bool isSnapshotExist = !string.IsNullOrWhiteSpace(comm.snapshotFileName);
                            bool isViewpointExist = !string.IsNullOrWhiteSpace(comm.viewpointFileName);
                            if(isSnapshotExist || isViewpointExist)
                            {
                                Guid tempGuid = Guid.Empty;
                                if (comm.snapshotFileName.Count() > 36)  //try to use the file name of snapshots as guid
                                {
                                    if(Guid.TryParse(comm.snapshotFileName.Substring(0, 36), out tempGuid))
                                        vp.Guid = tempGuid.ToString();
                                }                                
                                if (isViewpointExist)
                                {
                                    vp.Viewpoint = comm.viewpointFileName;
                                }
                                if (isSnapshotExist)
                                {                                
                                    vp.Snapshot = comm.snapshotFileName;
                                }

                                bcf2Comment.Viewpoint = new BCF2.CommentViewpoint() { Guid = vp.Guid };
                                bcf2ViewPoints.Add(vp);
                            }                                
                            
                            bcf2Comments.Add(bcf2Comment);                                                            
                        }
                    }

                    // Add document references
                    List<BCF2.TopicDocumentReferences> docs = new List<BCF2.TopicDocumentReferences>();
                    issue.fields.attachment.ForEach(file => {

                        Guid tempGuid = Guid.Empty;
                        if (!file.filename.ToLower().Contains(".png") && !file.filename.ToLower().Contains(".bcfv")) 
                        {
                            docs.Add(new BCF2.TopicDocumentReferences() { ReferencedDocument = file.content, Description = string.Format("{0} was uploaded to Jira by {1} at {2}", file.filename, file.author.displayName, file.created), isExternal = true, Guid = Guid.NewGuid().ToString() });
                        }
                        else if (file.filename.ToLower().Contains(".png"))
                        {
                            if (file.filename.Count() > 36)
                            {
                                if (!Guid.TryParse(file.filename.Substring(0, 36), out tempGuid))
                                    docs.Add(new BCF2.TopicDocumentReferences() { ReferencedDocument = file.content, Description = string.Format("{0} was uploaded to Jira by {1} at {2}", file.filename, file.author.displayName, file.created), isExternal = true, Guid = Guid.NewGuid().ToString() });
                            }
                            else 
                            {
                                if(file.filename != "snapshot.png")
                                    docs.Add(new BCF2.TopicDocumentReferences() { ReferencedDocument = file.content, Description = string.Format("{0} was uploaded to Jira by {1} at {2}", file.filename, file.author.displayName, file.created), isExternal = true, Guid = Guid.NewGuid().ToString() });
                            }                            
                        }
                            
                    });
                   
                    // Convert Topic
                    BCF2.Topic bcf2Topic = new BCF2.Topic()
                    {
                        AssignedTo = issue.fields.assignee == null ? null : issue.fields.assignee.name,
                        BimSnippet = null,
                        CreationAuthor = issue.fields.creator == null ? null : issue.fields.creator.displayName,
                        CreationDate = string.IsNullOrWhiteSpace(issue.fields.created) ? DateTime.Now : DateTime.Parse(issue.fields.created),
                        CreationDateSpecified = true,
                        Description = issue.fields.description == null ? null : issue.fields.description,
                        DocumentReferences = docs.Count == 0 ? null : docs.ToArray(),
                        Guid = issueGuid,
                        Index = null,
                        Labels = issue.fields.labels == null ? null : issue.fields.labels.ToArray(),
                        ModifiedAuthor = issue.fields.creator == null ? null : issue.fields.creator.displayName,
                        ModifiedDate = string.IsNullOrWhiteSpace(issue.fields.updated) ? DateTime.Now : DateTime.Parse(issue.fields.updated),
                        ModifiedDateSpecified = true,
                        Priority = issue.fields.priority == null ? null : issue.fields.priority.name,
                        ReferenceLink = string.Format("http://testingjiravdc.atlassian.net/browse/{0}", issue.key), 
                        RelatedTopics = null,
                        Title = issue.fields.summary == null ? null : issue.fields.summary,
                        TopicStatus = issue.fields.status == null ? null : issue.fields.status.name,
                        TopicType = issue.fields.issuetype == null ? null : issue.fields.issuetype.name
                    };                    

                    // Add BCF 2.0 issues/markups
                    bcf2.Issues.Add(new BCF2.Markup()
                    {
                        Header = bcf2Headers,
                        Comment = bcf2Comments,
                        Topic = bcf2Topic,
                        Viewpoints = bcf2ViewPoints
                    });

                    // Save all viewpoints/snapshots
                    try
                    {
                        foreach (var attachment in issue.fields.attachment) 
                        {
                            if (attachment.filename.ToLower().Contains(".png") || attachment.filename.ToLower().Contains(".bcfv") || attachment.filename.ToLower().Contains(".jpg") || attachment.filename.ToLower().Contains(".jpeg") || attachment.filename.ToLower().Contains(".bmp"))
                                mainPan.saveSnapshotViewpoint(attachment.content, Path.Combine(ReportFolder, issueGuid, attachment.filename));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to download viewpoint.bcfv and snapshot.png on Jira",
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }                    
                }

                if (errors != 0)
                {
                    MessageBox.Show(errors + " Issue(s) were not exported because only issues created via the issue tracker plugin with a viewpoint and a snapshot can be exported.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                    if (errors == mainPan.jiraPan.issueList.SelectedItems.Count)
                    {
                        mainPan.DeleteDirectory(ReportFolder);
                        return;
                    }
                }

                // Save BCF 2.0 file
                BCF2.BcfContainer.SaveBcfFile(bcf2);
            }
            catch (Exception ex)
            {
                // Get stack trace for the exception with source file information
                var st = new System.Diagnostics.StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                MessageBox.Show("Exception:" + line + "=====" + ex.ToString());
            }
        }

        /// <summary>
        /// Load from BCF 2.0 as BCF 1.0
        /// </summary>
        /// <param name="bcf2">BCF 2.0 file</param>
        /// <returns></returns>
        public static BCF1.IssueBCF LoadBcf1IssueFromBcf2(BCF2.Markup bcf2Markup, BCF2.VisualizationInfo bcf2Viewpoint)
        {
            // Convert headers            
            List<BCF1.HeaderFile> bcf1Headers = new List<BCF1.HeaderFile>();            
            foreach (BCF2.HeaderFile bcf2Header in bcf2Markup.Header)    
            {
                BCF1.HeaderFile bcf1Header = new BCF1.HeaderFile()
                {
                    Filename = bcf2Header.Filename,
                    Date = bcf2Header.Date,
                    IfcProject = bcf2Header.IfcProject,
                    IfcSpatialStructureElement = bcf2Header.IfcSpatialStructureElement
                };
                bcf1Headers.Add(bcf1Header);
            }

            // Convert topic
            BCF1.Topic bcf1Topic = new BCF1.Topic();
            if (bcf2Markup.Topic != null)
            {
                bcf1Topic.Guid = bcf2Markup.Topic.Guid;
                bcf1Topic.ReferenceLink = bcf2Markup.Topic.ReferenceLink; 
                bcf1Topic.Title = bcf2Markup.Topic.Title;
            };

            // Convert comments
            ObservableCollection<BCF1.CommentBCF> bcf1Comments = new ObservableCollection<BCF1.CommentBCF>();
            foreach(BCF2.Comment bcf2Comment in bcf2Markup.Comment)
            {
                BCF1.CommentBCF bcf1Comment = new BCF1.CommentBCF()
                {
                    Author = bcf2Comment.Author, 
                    Comment1 = bcf2Comment.Comment1, 
                    Date = bcf2Comment.Date, 
                    Guid = bcf2Comment.Guid,
                    Status = BCF1.CommentStatus.Unknown,    // default unknown for now
                    Topic = new BCF1.CommentTopic() { Guid = bcf2Markup.Topic == null ? Guid.NewGuid().ToString() : bcf2Markup.Topic.Guid }, 
                    VerbalStatus = bcf2Comment.VerbalStatus
                };
                bcf1Comments.Add(bcf1Comment);
            }

            // Convert markups/issues
            BCF1.Markup bcf1Markup = new BCF1.Markup()
            {
                Header = bcf1Headers.ToArray(),
                Topic = bcf1Topic,
                Comment = bcf1Comments
            };

            // Convert ClippingPlane
            List<BCF1.ClippingPlane> bcf1ClippingPlanes = new List<BCF1.ClippingPlane>();
            if (bcf2Viewpoint.ClippingPlanes != null)
            {                        
                foreach (BCF2.ClippingPlane bcf2ClippingPlane in bcf2Viewpoint.ClippingPlanes)
                {
                    if (bcf2ClippingPlane != null)
                    {
                        bcf1ClippingPlanes.Add(new BCF1.ClippingPlane()
                        {
                            Direction = new BCF1.Direction()
                            {
                                X = bcf2ClippingPlane.Direction.X,
                                Y = bcf2ClippingPlane.Direction.Y,
                                Z = bcf2ClippingPlane.Direction.Z
                            },
                            Location = new BCF1.Point()
                            {
                                X = bcf2ClippingPlane.Location.X,
                                Y = bcf2ClippingPlane.Location.Y,
                                Z = bcf2ClippingPlane.Location.Z
                            }
                        });
                    }
                }
            }

            // Convert Components
            List<BCF1.Component> bcf1Components = new List<BCF1.Component>();
            if (bcf2Viewpoint.Components != null)
            {                        
                foreach (BCF2.Component bcf2Component in bcf2Viewpoint.Components)
                {
                    if (bcf2Component != null)
                    {
                        bcf1Components.Add(new BCF1.Component()
                        {
                            AuthoringToolId = bcf2Component.AuthoringToolId,
                            IfcGuid = bcf2Component.IfcGuid,
                            OriginatingSystem = bcf2Component.OriginatingSystem
                        });
                    }
                }
            }

            // Convert Lines
            List<BCF1.Line> bcf1Lines = new List<BCF1.Line>();
            if (bcf2Viewpoint.Lines != null)
            {                        
                foreach (BCF2.Line bcf2Line in bcf2Viewpoint.Lines)
                {
                    if (bcf2Line != null)
                    {
                        bcf1Lines.Add(new BCF1.Line()
                        {
                            StartPoint = new BCF1.Point()
                            {
                                X = bcf2Line.StartPoint.X,
                                Y = bcf2Line.StartPoint.Y,
                                Z = bcf2Line.StartPoint.Z
                            },
                            EndPoint = new BCF1.Point()
                            {
                                X = bcf2Line.EndPoint.X,
                                Y = bcf2Line.EndPoint.Y,
                                Z = bcf2Line.EndPoint.Z
                            }
                        });
                    }
                }
            }

            // Convert viewpoints
            BCF1.VisualizationInfo bcf1Viewpoint = new BCF1.VisualizationInfo()
            {
                ClippingPlanes = bcf1ClippingPlanes.ToArray(), 
                Components = bcf1Components.ToArray(), 
                Lines = bcf1Lines.ToArray(),
                OrthogonalCamera = bcf2Viewpoint.OrthogonalCamera == null ? null : new BCF1.OrthogonalCamera()
                {
                    CameraDirection = new BCF1.Direction()
                    {
                        X = bcf2Viewpoint.OrthogonalCamera.CameraDirection.X,
                        Y = bcf2Viewpoint.OrthogonalCamera.CameraDirection.Y,
                        Z = bcf2Viewpoint.OrthogonalCamera.CameraDirection.Z
                    },
                    CameraUpVector = new BCF1.Direction()
                    {
                        X = bcf2Viewpoint.OrthogonalCamera.CameraUpVector.X,
                        Y = bcf2Viewpoint.OrthogonalCamera.CameraUpVector.Y,
                        Z = bcf2Viewpoint.OrthogonalCamera.CameraUpVector.Z
                    },
                    CameraViewPoint = new BCF1.Point()
                    {
                        X = bcf2Viewpoint.OrthogonalCamera.CameraViewPoint.X,
                        Y = bcf2Viewpoint.OrthogonalCamera.CameraViewPoint.Y,
                        Z = bcf2Viewpoint.OrthogonalCamera.CameraViewPoint.Z
                    },
                    ViewToWorldScale = bcf2Viewpoint.OrthogonalCamera.ViewToWorldScale
                },
                PerspectiveCamera = bcf2Viewpoint.PerspectiveCamera == null ? null : new BCF1.PerspectiveCamera()
                {
                    CameraDirection = new BCF1.Direction()
                    {
                        X = bcf2Viewpoint.PerspectiveCamera.CameraDirection.X,
                        Y = bcf2Viewpoint.PerspectiveCamera.CameraDirection.Y,
                        Z = bcf2Viewpoint.PerspectiveCamera.CameraDirection.Z
                    },
                    CameraUpVector = new BCF1.Direction()
                    {
                        X = bcf2Viewpoint.PerspectiveCamera.CameraUpVector.X,
                        Y = bcf2Viewpoint.PerspectiveCamera.CameraUpVector.Y,
                        Z = bcf2Viewpoint.PerspectiveCamera.CameraUpVector.Z
                    },
                    CameraViewPoint = new BCF1.Point()
                    {
                        X = bcf2Viewpoint.PerspectiveCamera.CameraViewPoint.X,
                        Y = bcf2Viewpoint.PerspectiveCamera.CameraViewPoint.Y,
                        Z = bcf2Viewpoint.PerspectiveCamera.CameraViewPoint.Z
                    },
                    FieldOfView = bcf2Viewpoint.PerspectiveCamera.FieldOfView
                },
                SheetCamera = bcf2Viewpoint.SheetCamera == null ? null : new BCF1.SheetCamera()
                {
                    SheetID = bcf2Viewpoint.SheetCamera.SheetID,
                    TopLeft = new BCF1.Point()
                    {
                        X = bcf2Viewpoint.SheetCamera.TopLeft.X,
                        Y = bcf2Viewpoint.SheetCamera.TopLeft.Y,
                        Z = bcf2Viewpoint.SheetCamera.TopLeft.Z
                    },
                    BottomRight = new BCF1.Point()
                    {
                        X = bcf2Viewpoint.SheetCamera.BottomRight.X,
                        Y = bcf2Viewpoint.SheetCamera.BottomRight.Y,
                        Z = bcf2Viewpoint.SheetCamera.BottomRight.Z
                    }
                }
            };

            // Create a new BCF 1.0 issue
            BCF1.IssueBCF bcf1 = new BCF1.IssueBCF()
            {
                markup = bcf1Markup,
                viewpoint = bcf1Viewpoint,
                bcf2Markup = bcf2Markup,
                bcf2Viewpoint = bcf2Viewpoint,
                guid = Guid.Parse(bcf2Markup.Topic.Guid)
            };

            return bcf1;
        }


        /// <summary>
        /// Write ExtensionSchema.xsd to the temp folder using available types, statuses, and priorities in a Jira project.
        /// </summary>
        private static void WriteExtensionSchema(string path, List<string> types, List<string> statuses, List<string> priorities)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "ARUP.IssueTracker.Classes.BCF2.ExtensionSchema.xsd";

                Stream stream = assembly.GetManifestResourceStream(resourceName);
                XmlTextReader reader = new XmlTextReader(stream);
                XmlSchema myschema = XmlSchema.Read(reader, ValidationCallback);

                XmlSchemaRedefine redefine = myschema.Includes[0] as XmlSchemaRedefine;
                if (redefine != null)
                {
                    foreach(XmlSchemaSimpleType simpleType in redefine.Items)
                    {
                        if (simpleType.Name == "TopicType")
                        {
                            simpleType.Content = GetXmlSchemaSimpleTypeRestriction(types);
                        }
                        else if (simpleType.Name == "TopicStatus")
                        {
                            simpleType.Content = GetXmlSchemaSimpleTypeRestriction(statuses);
                        }
                        else if (simpleType.Name == "Priority")
                        {
                            simpleType.Content = GetXmlSchemaSimpleTypeRestriction(priorities);
                        }
                    }
                }

                FileStream file = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                XmlTextWriter xwriter = new XmlTextWriter(file, new UTF8Encoding());
                xwriter.Formatting = Formatting.Indented;
                myschema.Write(xwriter);

                stream.Close();
                file.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("exception: " + e);
            }
        }

        private static XmlSchemaSimpleTypeRestriction GetXmlSchemaSimpleTypeRestriction(List<string> enumerations) 
        {
            XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction();
            enumerations.ForEach(
                e => restriction.Facets.Add(new XmlSchemaEnumerationFacet() { Value = e })
            );

            return restriction;            
        }

        private static void ValidationCallback(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                MessageBox.Show("WARNING: ");
            else if (args.Severity == XmlSeverityType.Error)
                MessageBox.Show("ERROR: ");

            MessageBox.Show(args.Message);
        }

        public static BCF2.ClippingPlane[] GetClippingPlanesFromBoundingBox(double maxX, double maxY, double maxZ, double minX, double minY, double minZ) 
        {
            List<BCF2.ClippingPlane> clippingPlanes = new List<BCF2.ClippingPlane>();
            BCF2.ClippingPlane xPositive = new BCF2.ClippingPlane()
            {
                Direction = new BCF2.Direction() { X = 1, Y = 0, Z = 0 },
                Location = new IssueTracker.Classes.BCF2.Point() { X = maxX, Y = maxY, Z = maxZ }
            };

            BCF2.ClippingPlane yPositive = new BCF2.ClippingPlane()
            {
                Direction = new BCF2.Direction() { X = 0, Y = 1, Z = 0 },
                Location = new IssueTracker.Classes.BCF2.Point() { X = maxX, Y = maxY, Z = maxZ }
            };

            BCF2.ClippingPlane zPositive = new BCF2.ClippingPlane()
            {
                Direction = new BCF2.Direction() { X = 0, Y = 0, Z = 1 },
                Location = new IssueTracker.Classes.BCF2.Point() { X = maxX, Y = maxY, Z = maxZ }
            };

            BCF2.ClippingPlane xNegative = new BCF2.ClippingPlane()
            {
                Direction = new BCF2.Direction() { X = -1, Y = 0, Z = 0 },
                Location = new IssueTracker.Classes.BCF2.Point() { X = minX, Y = minY, Z = minZ }
            };

            BCF2.ClippingPlane yNegative = new BCF2.ClippingPlane()
            {
                Direction = new BCF2.Direction() { X = 0, Y = -1, Z = 0 },
                Location = new IssueTracker.Classes.BCF2.Point() { X = minX, Y = minY, Z = minZ }
            };

            BCF2.ClippingPlane zNegative = new BCF2.ClippingPlane()
            {
                Direction = new BCF2.Direction() { X = 0, Y = 0, Z = -1 },
                Location = new IssueTracker.Classes.BCF2.Point() { X = minX, Y = minY, Z = minZ }
            };

            clippingPlanes.Add(xPositive);
            clippingPlanes.Add(yPositive);
            clippingPlanes.Add(zPositive);
            clippingPlanes.Add(xNegative);
            clippingPlanes.Add(yNegative);
            clippingPlanes.Add(zNegative);

            return clippingPlanes.ToArray();
        }

        public static BCF2.Point GetBoundingBoxMaxPointFromClippingPlanes(BCF2.ClippingPlane[] clippingPlanes) 
        {
            List<BCF2.ClippingPlane> clippingPlanesList = clippingPlanes.ToList();

            BCF2.ClippingPlane xPositive = clippingPlanesList.Find(cp => (cp.Direction.X == 1 && cp.Direction.Y == 0 && cp.Direction.Z == 0));
            BCF2.ClippingPlane yPositive = clippingPlanesList.Find(cp => (cp.Direction.X == 0 && cp.Direction.Y == 1 && cp.Direction.Z == 0));
            BCF2.ClippingPlane zPositive = clippingPlanesList.Find(cp => (cp.Direction.X == 0 && cp.Direction.Y == 0 && cp.Direction.Z == 1));

            if (xPositive != null && yPositive != null && zPositive != null)
            {
                if (isThreePointsEqual(xPositive.Location, yPositive.Location, zPositive.Location))
                {
                    return xPositive.Location;
                }
            }

            return null;
        }

        public static BCF2.Point GetBoundingBoxMinPointFromClippingPlanes(BCF2.ClippingPlane[] clippingPlanes)
        {
            List<BCF2.ClippingPlane> clippingPlanesList = clippingPlanes.ToList();

            BCF2.ClippingPlane xNegative = clippingPlanesList.Find(cp => (cp.Direction.X == -1 && cp.Direction.Y == 0 && cp.Direction.Z == 0));
            BCF2.ClippingPlane yNegative = clippingPlanesList.Find(cp => (cp.Direction.X == 0 && cp.Direction.Y == -1 && cp.Direction.Z == 0));
            BCF2.ClippingPlane zNegative = clippingPlanesList.Find(cp => (cp.Direction.X == 0 && cp.Direction.Y == 0 && cp.Direction.Z == -1));

            if (xNegative != null && yNegative != null && zNegative != null)
            {
                if (isThreePointsEqual(xNegative.Location, yNegative.Location, zNegative.Location))
                {
                    return xNegative.Location;
                }
            }

            return null;
        }

        private static bool isThreePointsEqual(BCF2.Point point1, BCF2.Point point2, BCF2.Point point3) 
        {
            bool isPoint1Point2Equal = point1.X == point2.X && point1.Y == point2.Y && point1.Z == point2.Z;
            bool isPoint1Point3Equal = point1.X == point3.X && point1.Y == point3.Y && point1.Z == point3.Z;

            return isPoint1Point2Equal && isPoint1Point3Equal;
        }
    }
}

﻿using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;

namespace HOK.Core.ElementWrapers
{
    public sealed class ImageTypeWrapper : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public int Instances { get; set; }
        public ElementId Id { get; set; }
        public BitmapImage BitmapImage { get; set; }

        public ImageTypeWrapper()
        {
        }

        public ImageTypeWrapper(ImageType type)
        {
            Name = type.Name;
            Id = type.Id;
            FilePath = type.Path;

            if (File.Exists(type.Path))
            {
                BitmapImage = new BitmapImage(new Uri(type.Path));
            }
            else
            {
                var bitmap = type.GetImage();
                BitmapImage = ImageUtilities.BitmapToBitmapImage(bitmap);
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged("IsSelected"); }
        }

        private bool _hasInstances;
        public bool HasInstances
        {
            get
            {
                _hasInstances = Instances > 0;
                return _hasInstances;
            }
            set { _hasInstances = value; RaisePropertyChanged("HasInstances"); }
        }

        public override bool Equals(object obj)
        {
            var item = obj as ImageTypeWrapper;
            return item != null && Id.Equals(item.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}

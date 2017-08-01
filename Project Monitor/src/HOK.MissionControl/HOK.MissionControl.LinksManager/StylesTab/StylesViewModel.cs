﻿using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.ElementWrapers;

namespace HOK.MissionControl.LinksManager.StylesTab
{
    public class StylesViewModel : ViewModelBase
    {
        public StylesModel Model { get; set; }
        public ObservableCollection<CategoryWrapper> Styles { get; set; }
        public RelayCommand Delete { get; }
        public RelayCommand<UserControl> Close { get; }

        public StylesViewModel(StylesModel model)
        {
            Model = model;
            Styles = Model.Styles;

            Delete = new RelayCommand(OnDelete);
            Close = new RelayCommand<UserControl>(OnClose);
        }

        private void OnDelete()
        {
            var deleted = Model.Delete(Styles);

            foreach (var i in deleted)
            {
                Styles.Remove(i);
            }
        }

        private static void OnClose(UserControl control)
        {
            var win = Window.GetWindow(control);
            win?.Close();
        }
    }
}
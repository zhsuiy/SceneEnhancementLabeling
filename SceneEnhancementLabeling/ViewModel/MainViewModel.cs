using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;
using SceneEnhancementLabeling.Models;

namespace SceneEnhancementLabeling.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private RelayCommand _loadCategoryCommand;

        public ICommand LoadCategoryCommand => _loadCategoryCommand ?? (_loadCategoryCommand = new RelayCommand(LoadCategory));

        private void LoadCategory()
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (var stream = dialog.OpenFile())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        try
                        {
                            var content = reader.ReadToEnd();
                            var list = JsonConvert.DeserializeObject<List<CategoryItem>>(content);
                            
                            var labeling = ServiceLocator.Current.GetInstance<LabelingViewModel>();
                            if (labeling != null)
                            {
                                labeling.Category = new ObservableCollection<CategoryItem>(list);
                                labeling.CategoryIndex = 0;
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
            }
        }
    }
}
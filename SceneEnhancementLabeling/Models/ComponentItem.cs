using System.Collections.Generic;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace SceneEnhancementLabeling.Models
{
    public class ComponentItem : ObservableObject
    {
        private int _id;

        [JsonProperty("id")]
        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged();
            }
        }

        private string _name;

        [JsonProperty("name")]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        private string _nameCn;

        [JsonProperty("name_cn")]
        public string NameCn
        {
            get { return _nameCn; }
            set
            {
                _nameCn = value;
                RaisePropertyChanged();
            }
        }

        private bool _isDirty;

        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;
                RaisePropertyChanged();
            }
        }

        private List<CategoryItem> _category;

        public List<CategoryItem> Category
        {
            get { return _category; }
            set
            {
                _category = value;
                RaisePropertyChanged();
            }
        }

        private int _selecedCategoryIndex = -1;

        public int SelectedCategoryIndex
        {
            get { return _selecedCategoryIndex; }
            set
            {
                _selecedCategoryIndex = value;
                RaisePropertyChanged();
                if (value != -1)
                {
                    IsDirty = true;
                }
            }
        }

        private bool _isLeft;

        public bool IsLeft
        {
            get
            {
                return _isLeft;
            }
            set
            {
                _isLeft = value;
                RaisePropertyChanged();
                if (!CheckTwoOfThree() && value)
                {
                    System.Windows.MessageBox.Show("Cannot set more than 2 directions.");
                    IsLeft = false;
                }
                else
                {
                    IsDirty = true;
                }
            }
        }

        private bool _isRight;

        public bool IsRight
        {
            get
            {
                return _isRight;
            }
            set
            {
                _isRight = value;
                RaisePropertyChanged();
                if (!CheckTwoOfThree() && value)
                {
                    System.Windows.MessageBox.Show("Cannot set more than 2 directions.");
                    IsRight = false;
                }
                else
                {
                    IsDirty = true;
                }
            }
        }

        private bool _isFront;

        public bool IsFront
        {
            get
            {
                return _isFront;
            }
            set
            {
                _isFront = value;
                RaisePropertyChanged();
                if (!CheckTwoOfThree() && value)
                {
                    System.Windows.MessageBox.Show("Cannot set more than 2 directions.");
                    IsFront = false;
                }
                else
                {
                    IsDirty = true;
                }
            }
        }

        private bool _isBack;

        public bool IsBack
        {
            get
            {
                return _isBack;
            }
            set
            {
                _isBack = value;
                RaisePropertyChanged();
                if (!CheckTwoOfThree() && value)
                {
                    System.Windows.MessageBox.Show("Cannot set more than 2 directions.");
                    IsBack = false;
                }
                else
                {
                    IsDirty = true;
                }
            }
        }

        private bool _isCenter;

        public bool IsCenter
        {
            get
            {
                return _isCenter;
            }
            set
            {
                _isCenter = value;
                RaisePropertyChanged();
                if (!CheckTwoOfThree() && value)
                {
                    System.Windows.MessageBox.Show("Cannot set more than 2 directions.");
                    IsCenter = false;
                }
                else
                {
                    IsDirty = true;
                }
            }
        }

        private bool CheckTwoOfThree()
        {
            int num = 0;
            if (IsLeft || IsRight)
            {
                num++;
            }
            if (IsFront || IsBack)
            {
                num++;
            }
            if (IsCenter)
            {
                num++;
            }
            return num <= 2;
        }
    }
}

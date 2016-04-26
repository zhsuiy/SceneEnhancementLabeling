using System.Collections.Generic;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace SceneEnhancementLabeling.Models
{
    public class GroupedComponentsCollection : List<GroupedComponents>
    {
        
    }

    public class GroupedComponents : ObservableObject
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("data")]
        public List<ComponentItem> Data { get; set; }

        private int _selectedComponentIndex = -1;

        public int SelectedComponentIndex
        {
            get { return _selectedComponentIndex; }
            set
            {
                _selectedComponentIndex = value;
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

        private int _selectedCategoryIndex = -1;

        public int SelectedCategoryIndex
        {
            get { return _selectedCategoryIndex; }
            set
            {
                _selectedCategoryIndex = value;
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
                    ErrorMessage = "Cannot set more than 2 directions.";
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
                    ErrorMessage = "Cannot set more than 2 directions.";
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
                    ErrorMessage = "Cannot set more than 2 directions.";
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
                    ErrorMessage = "Cannot set more than 2 directions.";
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
                    ErrorMessage = "Cannot set more than 2 directions.";
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
            if (num == 0)
            {
                ErrorMessage = "";
            }
            return num <= 2;
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                RaisePropertyChanged();
            }
        }
    }
}

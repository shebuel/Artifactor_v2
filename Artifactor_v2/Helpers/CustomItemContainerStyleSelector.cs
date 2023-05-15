using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Artifactor_v2.Models;

namespace Artifactor_v2.Helpers;

    public class CustomItemContainerStyleSelector : StyleSelector
    {
        public Style? Checked
        {
            get; set;
        }
        public Style? UnChecked
        {
            get; set;
        }

        protected override Style SelectStyleCore(object item, DependencyObject container)
        {

            ObservableCheck v = (ObservableCheck)item;
            if (v.CheckCompleted == true)
            {
                return Checked;
            }
            else
            {
                return UnChecked;
            }

            return base.SelectStyleCore(item, container);
        }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion_activite
{
    public class HoraireSelection
    {
        private static HoraireSelection SelectedHoraire { get; set; }

        public string Horaire { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;

                    if (_isSelected)
                    {
                        if (SelectedHoraire != null && SelectedHoraire != this)
                        {
                            SelectedHoraire.IsSelected = false;
                        }
                        SelectedHoraire = this;
                    }
                    else if (SelectedHoraire == this)
                    {
                        SelectedHoraire = null;
                    }
                }
            }
        }
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FSPOC2.Models
{
    public partial class Application
    {
        [Required]
        [Display(Name = "Name")]
        public string _Name
        {
            get { return Name; }
            set { Name = value; }
        }

        [Required]
        [Display(Name = "Database tables prefix")]
        public string _DbTablePrefix
        {
            get { return DbTablePrefix; }
            set { DbTablePrefix = value; }
        }

        [Required]
        [Display(Name = "Database table for metadata")]
        public string _DbMetaTables
        {
            get { return DbMetaTables; }
            set { DbMetaTables = value; }
        }
    }
}
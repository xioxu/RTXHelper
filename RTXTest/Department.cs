using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTXTest
{
    public class Department
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public List<Department> ChildDepartments { get; set; }
    }
}

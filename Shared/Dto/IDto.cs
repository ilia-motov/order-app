using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApp.Shared.Dto
{
    public interface IDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

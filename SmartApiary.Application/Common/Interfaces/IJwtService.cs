using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Application.Common.Interfaces;

using SmartApiary.Domain.Models;

public interface IJwtService
{
    string GenerateToken(User user);
}
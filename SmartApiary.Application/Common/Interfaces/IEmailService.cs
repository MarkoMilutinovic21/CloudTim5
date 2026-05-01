using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartApiary.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendActivationEmailAsync(string to, string token, CancellationToken ct = default);
    }
}



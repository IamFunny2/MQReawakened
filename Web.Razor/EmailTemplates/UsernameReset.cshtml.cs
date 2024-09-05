using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;
using Server.Reawakened.Core.Configs;

namespace Web.Razor.EmailTemplates;

public class UsernameResetModel(ServerRwConfig config, InternalRwConfig iConfig) : PageModel
{
    public string Email { get; set; }
    public string ResetLink { get; set; }

    public string Domain => config.DomainName;
    public string SiteName => iConfig.ServerName;
}
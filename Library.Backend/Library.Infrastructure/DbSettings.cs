using System;

namespace Library.Infrastructure;

public class DbSettings
{
    public string User { get; set; }
    public string Password { get; set; }
    
    public string Server { get; set; }
    public string Database { get; set; }
}

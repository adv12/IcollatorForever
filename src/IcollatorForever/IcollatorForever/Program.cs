// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the IcollatorForever distribution or repository for the
// full text of the license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Hosting;

namespace IcollatorForever
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = WebAssemblyHostBuilder.CreateDefault(args);
            hostBuilder.RootComponents.Add<App>("app");
            await hostBuilder.Build().RunAsync();
        }
    }
}

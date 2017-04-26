using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Wiki.ServiceHost.Helpers;

namespace Wiki.ServiceHost.Nuget
{
    [Serializable]
    internal class ArgumentParser
    {
        private readonly List<string> _args;
        private string _nugetDir;
        private string _error;
        private string _id;
        private string _version;
        private int _port;

        public ArgumentParser(string[] args)
        {
            this._args = args.ToList();
            
        }

        public string[] Arguments { get { return this._args.ToArray(); } }

        public string NugetDir
        {
            get { return this._nugetDir; }
        }

        public string Error
        {
            get { return this._error; }
        }

        public string Id
        {
            get { return this._id; }
        }

        public int Port
        {
            get { return this._port; }
        }

        public string Version
        {
            get { return this._version; }
        }
        public void Parse()
        {
            ParsePort();
            var res= this.GetDir()
                   &&this.GetId()
                   &&this.GetVersion();
            if (!res)
                throw new NugetNoadException(this.Error);

        }
        private bool GetVersion()
        {
            var idx = this._args.IndexOf("-v");
            if (idx < 0 || this._args.Count <= idx)
            {
                this._error = "Incorrect version nuget package. Set it: -v \"version\".\nUsed last version.";
                return true;
            }
            this._version = this._args[idx + 1];
            return true;

        }

        private bool GetId()
        {
            var idx = this._args.IndexOf("-id");
            if (idx < 0 || this._args.Count <= idx)
            {
                this._error = "Incorrect Id nuget package. Set it: -id \"idPackage\".";
                return false;
            }
            this._id = this._args[idx + 1];
            return true;
        }

        private bool GetDir()
        {
            var idx = this._args.IndexOf("-d");
            if (idx == -1)
                idx = this._args.IndexOf("-dir");
            if (idx < 0 || this._args.Count <= idx)
                this._nugetDir=ConfigurationManager.AppSettings["nugetDir"];
            else
                this._nugetDir = this._args[idx + 1];

            var result = Directory.Exists(this._nugetDir);
            if (!result)
                this._error = string.Format("Incorrect nuger directory.\n({0})\nSet it with paramert [-d | -dir] \"directory\" \nor set app paramert \"nugetDir\" in config file.",this._nugetDir);
            return result;
        }

        private void ParsePort()
        {
            var idx = this._args.IndexOf("-port");
            if (idx < 0 || this._args.Count <= idx)
                return;
            var port= this._args[idx + 1];
            int.TryParse(port, out this._port);
        }

        
    }
}
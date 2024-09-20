using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using AtlasReaper.Options;
using System.Threading;

namespace AtlasReaper.Confluence
{
    internal class Spaces
    {
        // List spaces based on options
        internal void ListSpaces(ConfluenceOptions.ListSpacesOptions options)
        {
            try
            {
                List<Space> spaces = new List<Space>(); 

                if (options.Space != null)
                {
                    // Get a single space
                    Space space = GetSpace(options);
                    Space spacecnt = null;
                    if (space.Key.StartsWith("~")|| space.Type== "global" || space.Type == "personal")
                    {
                        //personal
                        spacecnt = GetSpacePersonalCnt(options);

                    }
                    else {
                         spacecnt = GetSpaceCnt(options);
                    }
                  
                    if (spacecnt.Page.Results.Any()) 
                    {
                        var pageoption = new ConfluenceOptions.ListPagesOptions();
                        pageoption.Space = options.Space;
                        pageoption.Cookie = options.Cookie;
                        pageoption.Url = options.Url;

                        if (space.Key.StartsWith("~"))
                        {
                            //personal
                            foreach (var item in spacecnt.Page.Results)
                            {
                                pageoption.Page = item.Id;
                                Pages.GetAllPages(pageoption);
                            }

                        }
                        else
                        {
                            if (space._expandable.homepage != null)
                            {
                                pageoption.Page = space._expandable.homepage.Replace("/rest/api/content/", "");

                                Pages.GetAllPages(pageoption);
                            }
                          
                            
                        }
                           


                    }
                    spaces.Add(space);
                }
                else if (options.AllSpaces)
                {
                    // List all spaces
                    spaces = GetAllSpaces(options);

                }
                else
                {
                    // List Spaces by limit
                    RootSpacesObject spacesList = GetSpaces(options);
                    spaces = spacesList.Results.ToList();
                    var validSpaces = new List<Space>();
                    foreach (var s in spaces)
                    {
                        options.Space = s.Key;
                        Space space = GetSpace(options);
                        space.Key = s.Key;
                        if (space.Page.Results.Any())
                        {
                            ////?limit=500&start=0
                            ///limit  500 is the max count 
                            space.Count = space.Page.Results.Count();
                            validSpaces.Add(space);
                        }
                       


                    }
                    spaces = validSpaces.OrderBy(o => o.Type).ToList();
                }
                if (options.Type != null)
                {
                    spaces = spaces.Where(space => space != null && space.Type == options.Type).ToList();
                }
                if (options.outfile != null)
                {
                    using (StreamWriter writer = new StreamWriter(options.outfile))
                    {
                        PrintSpaces(spaces, writer);
                    }
                }
                else
                {
                    PrintSpaces(spaces, Console.Out);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while listing spaces: " + ex.Message);
            }
            
            

        }

        // Get Spaces based on options
        internal static RootSpacesObject GetSpaces(ConfluenceOptions.ListSpacesOptions options, string paginationToken = null)
        {
            RootSpacesObject spaceList = new RootSpacesObject();
            var url = options.Url + "/rest/api/space?limit=" + options.Limit;
            if (paginationToken != null)
            {
                url += "&" + paginationToken;
            }

            try
            {
                Utils.WebRequestHandler webRequestHandler = new Utils.WebRequestHandler();

                spaceList = webRequestHandler.GetJson<RootSpacesObject>(url, options.Cookie);

                return spaceList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while getting spaces: " + ex.Message);
                return spaceList;
            }
           
        }

        // Get a single Space
        internal static Space GetSpace(ConfluenceOptions.ListSpacesOptions options)
        {
            Space space = new Space(); 
            string url = options.Url + "/rest/api/space/" + options.Space;
            try
            {
                Utils.WebRequestHandler webRequestHandler = new Utils.WebRequestHandler();
                space = webRequestHandler.GetJson<Space>(url, options.Cookie);
                return space;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while getting space: " + ex.Message);
                return space;
            }

        }
        internal static Space GetSpacePersonalCnt(ConfluenceOptions.ListSpacesOptions options, int start = 0)
        {
            Space space = new Space();
            string url = options.Url + "/rest/api/space/" + options.Space + "/content?limit=99999";
            try
            {
                Utils.WebRequestHandler webRequestHandler = new Utils.WebRequestHandler();
                space = webRequestHandler.GetJson<Space>(url, options.Cookie);
                if (space.Page.Results.Count() < 500)
                {
                    return space;
                }
                else
                {
                    var incre = 0;
                    var counter = 0;
                    List<Space> list = new List<Space>();
                    while (counter == 500|| incre == 0|| (counter < 500&& counter > 0)) 
                    {
                        url = options.Url + "/rest/api/space/" + options.Space + "/content?limit=99999&start=" + incre;
                        var slp = (int)((new Random()).NextDouble() * 1000);

                        Console.WriteLine("sleep: "+slp);
                       // Thread.Sleep(slp);
                      
                        Space spaceAppend = webRequestHandler.GetJson<Space>(url, options.Cookie);
                        counter = spaceAppend.Page.Results.Count();
                        incre += counter;
                        list.AddRange(spaceAppend.Page.Results);
                       
                    }
                    space.Page.Results = list;
                    return space;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while getting space: " + ex.Message);
                return space;
            }

        }
        internal static Space GetSpaceCnt(ConfluenceOptions.ListSpacesOptions options, int start = 0)
        {
            Space space = new Space();
            string url = options.Url + "/rest/api/space/" + options.Space + "/content/child?limit=99999";
            try
            {
                Utils.WebRequestHandler webRequestHandler = new Utils.WebRequestHandler();
                space = webRequestHandler.GetJson<Space>(url, options.Cookie);
                return space;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while getting space: " + ex.Message);
                return space;
            }

        }

        // Get All Spaces 
        internal static List<Space> GetAllSpaces(ConfluenceOptions.ListSpacesOptions options)
        {
            List<Space> spaces = new List<Space>();
            // Set limit to 250 to reduce number of requests
            options.Limit = "250";
            try
            {
                RootSpacesObject spacesList = GetSpaces(options);

                spaces = spacesList.Results;

                while (spacesList != null && spacesList._Links.Next != null)
                {
                    string nextToken = spacesList._Links.Next.Split('&').Last();

                    spacesList = GetSpaces(options, nextToken);
                    spaces.AddRange(spacesList.Results);
                }

                spaces = spaces.OrderBy(o => o.Type).ToList();

                return spaces;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while getting all spaces: " + ex.Message);
                return spaces;
            }

        }

        // Print Spaces information
        internal void PrintSpaces(List<Space> spaces, TextWriter writer)
        {
            try
            {
                for (int i = 0; i < spaces.Count; i++)
                {
                    Space space = spaces[i];
                    //writer.WriteLine("    Space Name  : " + space.Name);
                   // writer.WriteLine("    Space Id    : " + space.Id);
                    writer.WriteLine("    Space Key   : " + space.Key);
                    writer.WriteLine("    Space Cnt Count   : " + space.Count);
                    // writer.WriteLine("    Space Type  : " + space.Type);
                    //writer.WriteLine("Space Description: " + space.Description);
                    //writer.WriteLine("    Space Status: " + space.Status);
                    writer.WriteLine();
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while printing space: " + ex.Message);
            }

        }
    }

    internal class RootSpacesObject
    {
     

        [JsonProperty("results")]
        internal List<Space> Results { get; set; }

        [JsonProperty("_links")]
        internal _Links _Links { get; set; }
    }
    internal class _expandable
    {
        [JsonProperty("homepage")]
        internal string homepage { get; set; }
    }
    internal class Space
    {//_expandable/homepage
        //V7.0
        [JsonProperty("page")]
        internal Space Page { get; set; }
        [JsonProperty("results")]
        internal List<Space> Results { get; set; }
        [JsonProperty("_expandable")]
        internal _expandable _expandable { get; set; }

        [JsonProperty("title")]
        internal string Title { get; set; }

        [JsonProperty("id")]
        internal string Id { get; set; }

        [JsonProperty("key")]
        internal string Key { get; set; }

        [JsonProperty("name")]
        internal string Name { get; set; }

        [JsonProperty("type")]
        internal string Type { get; set; }

        [JsonProperty("status")]
        internal string Status { get; set; }

        [JsonProperty("homepageId")]
        internal string HomepageId { get; set; }

        [JsonProperty("description")]
        internal Description Description { get; set; }
        internal int Count { get; set; }
    }

    internal class Description
    {
        [JsonProperty("plain")]
        internal string Plain { get; set; }

        [JsonProperty("view")]
        internal string View { get; set; }
    }

    internal class _Links
    {
        [JsonProperty("base")]
        internal string Base { get; set; }

        [JsonProperty("next")]
        internal string Next { get; set; }
    }
}



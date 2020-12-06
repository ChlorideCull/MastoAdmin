using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace MastoAdmin.RemoteApi
{
    public class Scraping
    {
        protected CookieContainer _cookieContainer;
        protected HttpClient _client;
        protected Uri _domain;
        
        public Scraping(Uri domain)
        {
            _domain = domain;
            _cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler()
            {
                CookieContainer = _cookieContainer,
                UseCookies = true
            };
            _client = new HttpClient(handler);
        }

        protected async Task<string> FetchCsrfToken(Uri url, string formId)
        {
            var page = await _client.GetAsync(url);
            page.EnsureSuccessStatusCode();
            var doc = new HtmlDocument();
            doc.Load(await page.Content.ReadAsStreamAsync());
            return doc.GetElementbyId(formId).ChildNodes
                .Single(x => x.GetAttributeValue<string>("name", null) == "authenticity_token")
                .GetAttributeValue("value", null);
        }
        
        public async Task<bool> Login(string username, string password)
        {
            /*
             * GET /auth/sign_in, and grab the meta[name="csrf-token"] value as authenticity_token
             * POST /auth/sign_in with
             *  * utf8="✓"
             *  * authenticity_token=<authenticity_token>
             *  * user[email]=<username>
             *  * user[password]=<password>
             *  * button=""
             */
            var csrfToken = await FetchCsrfToken(new Uri(_domain.GetLeftPart(UriPartial.Authority) + "/auth/sign_in"), "new_user");
            
            var formData = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("utf8", "✓"),
                new KeyValuePair<string, string>("authenticity_token", csrfToken),
                new KeyValuePair<string, string>("user[email]", username),
                new KeyValuePair<string, string>("user[password]", password),
                new KeyValuePair<string, string>("button", "") 
            });
            var login = await _client.PostAsync(new Uri(_domain.GetLeftPart(UriPartial.Authority) + "/auth/sign_in"), formData);
            var loginResponse = await login.Content.ReadAsStringAsync();
            return !loginResponse.Contains("<title>Log in");
        }
        
        public async Task BlockDomain(string domain, string publicComment = "", string privateComment = "")
        {
            /* GET /admin/domain_blocks/new, and grab the meta[name="csrf-token"] value as authenticity_token
             * POST /admin/domain_blocks with
             *  * utf8="✓"
             *  * authenticity_token=<authenticity_token>
             *  * domain_block[domain]=<domain>
             *  * domain_block[severity]="suspend"
             *  * domain_block[reject_media]=0
             *  * domain_block[reject_reports]=0
             *  * domain_block[private_comment]=<privateComment>
             *  * domain_block[public_comment]=<publicComment>
             *  * button=""
             */
            var csrfToken = await FetchCsrfToken(new Uri(_domain.GetLeftPart(UriPartial.Authority) + "/admin/domain_blocks/new"), "new_domain_block");
            
            var formData = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("utf8", "✓"),
                new KeyValuePair<string, string>("authenticity_token", csrfToken),
                new KeyValuePair<string, string>("domain_block[domain]", domain),
                new KeyValuePair<string, string>("domain_block[severity]", "suspend"),
                new KeyValuePair<string, string>("domain_block[reject_media]", "0"),
                new KeyValuePair<string, string>("domain_block[reject_reports]", "0"),
                new KeyValuePair<string, string>("domain_block[private_comment]", privateComment),
                new KeyValuePair<string, string>("domain_block[public_comment]", publicComment),
                new KeyValuePair<string, string>("button", "") 
            });
            var executeBlock = await _client.PostAsync(new Uri(_domain.GetLeftPart(UriPartial.Authority) + "/admin/domain_blocks"), formData);
            executeBlock.EnsureSuccessStatusCode();
        }
    }
}
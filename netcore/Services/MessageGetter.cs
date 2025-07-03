using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ImageActivityMonitor.Services
{
    public class MessageGetter
    {
        private readonly HttpClient client;
        private readonly string urlBase;

        public MessageGetter(HttpClient client, string urlBase)
        {
            this.client = client;
            this.urlBase = urlBase;
        }

        public async Task<List<(int messageId, DateTime schedule)>> BuildAgenda(List<string> grupos)
        {
            var agendaTemp = new List<(int messageId, DateTime schedule)>();

            foreach (var grupo in grupos)
            {
                try
                {
                    var response = await client.GetAsync($"{urlBase}/search/messagesgroup/{grupo}");
                    if (!response.IsSuccessStatusCode) continue;

                    string content = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject<dynamic>(content);
                    foreach (var item in result.response)
                    {
                        int messageId = item.message_id;
                        DateTime schedule = item.schedule["$date"].ToObject<DateTime>();
                        agendaTemp.Add((messageId, schedule));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al obtener mensajes de {grupo}: {ex.Message}");
                }
            }

            var agenda = agendaTemp
                .OrderBy(x => x.messageId)
                .ThenBy(x => x.schedule)
                .GroupBy(x => x.messageId)
                .SelectMany(g =>
                {
                    var list = new List<(int messageId, DateTime schedule)>();
                    foreach (var item in g)
                    {
                        if (list.Count == 0 || (item.schedule - list.Last().schedule).TotalMinutes >= 30)
                        {
                            list.Add(item);
                        }
                    }
                    return list;
                }).ToList();

            return agenda;
        }

        public async Task<Dictionary<int, dynamic>> FetchMessages(List<(int messageId, DateTime schedule)> agenda)
        {
            var mensajes = new Dictionary<int, dynamic>();
            var ids = agenda.Select(a => a.messageId).Distinct();

            foreach (var id in ids)
            {
                try
                {
                    var response = await client.GetAsync($"{urlBase}/messages?id={id}");
                    if (!response.IsSuccessStatusCode) continue;

                    string content = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject<dynamic>(content);
                    mensajes[id] = result.response;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al obtener el mensaje {id}: {ex.Message}");
                }
            }

            return mensajes;
        }
    }
}

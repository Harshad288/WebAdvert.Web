using AdvertApi.Models;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebAdvert.Web.ServiceClients
{
    public class AdvertApiClient : IAdvertApiClient
    {
        private readonly IConfiguration _configuration;

        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        public AdvertApiClient(IConfiguration  configuration,HttpClient httpClient,IMapper mapper)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _mapper = mapper;
            var createUrl = _configuration.GetSection(key: "AdvertApi").GetValue<string>(key: "CreateUrl");
            _httpClient.BaseAddress = new Uri(createUrl);
            _httpClient.DefaultRequestHeaders.Add("content-type", "application/json");
        }

        public async Task<bool> Confirm(ConfirmAdvertRequest confirmAdvertModel)
        {
            var advertModel = _mapper.Map<ConfirmAdvertModel>(confirmAdvertModel);
            var jsonModel = JsonConvert.SerializeObject(advertModel);
            var response = await _httpClient.PutAsync(new Uri($"{_httpClient.BaseAddress}/confirm"), new StringContent(jsonModel));
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<AdvertResponse> Create(CreateAdvertModel advertModel)
        {
            var advertApiModel = _mapper.Map<AdvertModel>(advertModel);
            var jsonModel = JsonConvert.SerializeObject(advertModel);
            var response = await _httpClient.PostAsync(new Uri($"{_httpClient.BaseAddress}/create"), new StringContent(jsonModel));
            var responseJson = await response.Content.ReadAsStringAsync();
            var createAdvertResponse = JsonConvert.DeserializeObject<CreateAdvertResponse>(responseJson);
            var advertResponse = _mapper.Map<AdvertResponse>(createAdvertResponse);
            return advertResponse;
        }
    }
}

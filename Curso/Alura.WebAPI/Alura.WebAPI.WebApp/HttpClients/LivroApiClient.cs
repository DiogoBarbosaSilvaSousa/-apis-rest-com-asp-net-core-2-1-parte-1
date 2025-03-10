﻿using Alura.ListaLeitura.Modelos;
using Lista = Alura.ListaLeitura.Modelos.ListaLeitura;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Alura.ListaLeitura.Seguranca;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Alura.ListaLeitura.HttpClients
{
    public class LivroApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _accessor;

        public LivroApiClient(HttpClient client, IHttpContextAccessor accessor)
        {
            _httpClient = client;
            _accessor = accessor;
        }

        private void AddBearerToken()
        {
            var token = _accessor.HttpContext.User.Claims.First(c => c.Type == "Token").Value;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<byte[]> GetCapaLivroAsync(int id)
        {

            AddBearerToken();

            var resposta = await _httpClient.GetAsync($"livros/{id}/capa");
            resposta.EnsureSuccessStatusCode();
            return await resposta.Content.ReadAsByteArrayAsync();
        }

        public async Task<LivroApi> GetLivroAsync(int id)
        {
            AddBearerToken();
            var resposta = await _httpClient.GetAsync($"livros/{id}");
            resposta.EnsureSuccessStatusCode();
            return await resposta.Content.ReadAsAsync<LivroApi>();
        }

        public async Task DeleteLivroAsync(int id)
        {
            AddBearerToken();

            var resposta = await _httpClient.DeleteAsync($"livros/{id}");
            resposta.EnsureSuccessStatusCode();
            if (resposta.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException("Código de Status Http 204 esperado!");
            }
        }

        public async Task PostLivroAsync(LivroUpload livro)
        {
            AddBearerToken();
            HttpContent content = CreateMultipartContent(livro.ToLivro());
            var resposta = await _httpClient.PostAsync("livros", content);
            resposta.EnsureSuccessStatusCode();
            if (resposta.StatusCode != System.Net.HttpStatusCode.Created)
            {
                throw new InvalidOperationException("Código de Status Http 201 esperado!");
            }
        }

        public async Task PutLivroAsync(LivroUpload livro)
        {
            AddBearerToken();

            HttpContent content = CreateMultipartContent(livro.ToLivro());
            var resposta = await _httpClient.PutAsync("livros", content);
            resposta.EnsureSuccessStatusCode();
            if (resposta.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InvalidOperationException("Código de Status Http 200 esperado!");
            }

        }

        private string EnvolveComAspasDuplas(string valor)
        {
            return $"\u0022{valor}\u0022";
        }

        private HttpContent CreateMultipartContent(Livro livro)
        {
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(livro.Titulo), EnvolveComAspasDuplas("titulo"));
            content.Add(new StringContent(livro.Lista.ParaString()), EnvolveComAspasDuplas("lista"));

            if (livro.Id > 0)
            {
                content.Add(new StringContent(Convert.ToString(livro.Id)), EnvolveComAspasDuplas("id"));
            }

            if (!string.IsNullOrEmpty(livro.Subtitulo))
            {
                content.Add(new StringContent(livro.Subtitulo), EnvolveComAspasDuplas("subtitulo"));
            }

            if (!string.IsNullOrEmpty(livro.Resumo))
            {
                content.Add(new StringContent(livro.Resumo), EnvolveComAspasDuplas("resumo"));
            }

            if (!string.IsNullOrEmpty(livro.Autor))
            {
                content.Add(new StringContent(livro.Autor), EnvolveComAspasDuplas("autor"));
            }

            if (livro.ImagemCapa != null)
            {
                var imageContent = new ByteArrayContent(livro.ImagemCapa);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                content.Add(imageContent, EnvolveComAspasDuplas("capa"), EnvolveComAspasDuplas("capa.png"));
            }

            return content;
        }

        public async Task<Lista> GetListaLeituraAsync(TipoListaLeitura tipo)
        {
             AddBearerToken();

            var resposta = await _httpClient.GetAsync($"listasleitura/{tipo}");
            resposta.EnsureSuccessStatusCode();
            return await resposta.Content.ReadAsAsync<Lista>();
        }
    }
}

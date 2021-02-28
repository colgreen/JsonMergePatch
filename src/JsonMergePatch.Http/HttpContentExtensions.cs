﻿using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LaDeak.JsonMergePatch.Abstractions;

namespace LaDeak.JsonMergePatch.Http
{
    public static class HttpContentExtensions
    {
        private static JsonSerializerOptions _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public static Task<Patch<TResult>?> ReadJsonPatchAsync<TResult>(this HttpContent content, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default) =>
            content.ReadJsonPatchAsync<TResult>(JsonMergePatchOptions.Repository, options, cancellationToken);

        public static async Task<Patch<TResult>?> ReadJsonPatchAsync<TResult>(this HttpContent content, ITypeRepository typeRepository, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            if (typeRepository == null)
                throw new ArgumentNullException(nameof(typeRepository));
            if (!typeRepository.TryGet(typeof(TResult), out var wrapperType))
                throw new ArgumentException($"{typeof(TResult)} is missing generated wrapper type. Check if all members of the type definition is supported.");
            if (content.Headers.ContentType?.MediaType != "application/merge-patch+json" && content.Headers.ContentType?.MediaType != "application/json" && !string.IsNullOrWhiteSpace(content.Headers.ContentType?.MediaType))
                return null;

            var contentStream = await content.ReadAsStreamAsync().ConfigureAwait(false);
#if NET5_0
            Encoding? encoding = content.Headers.ContentType?.CharSet != null ? GetEncoding(content.Headers.ContentType.CharSet) : null;
            if (encoding != null && encoding != Encoding.UTF8)
                contentStream = Encoding.CreateTranscodingStream(contentStream, encoding, Encoding.UTF8);
#endif
            await using (contentStream.ConfigureAwait(false))
            {
                var contentData = await JsonSerializer.DeserializeAsync(contentStream, wrapperType, options ?? _serializerOptions, cancellationToken).ConfigureAwait(false);
                return contentData as Patch<TResult>;
            }
        }

        private static Encoding? GetEncoding(string? charset)
        {
            Encoding? encoding = null;

            if (charset != null)
            {
                if (charset.Length > 2 && charset[0] == '\"' && charset[charset.Length - 1] == '\"')
                    encoding = Encoding.GetEncoding(charset.Substring(1, charset.Length - 2));
                else
                    encoding = Encoding.GetEncoding(charset);
            }

            return encoding;
        }
    }
}

using System.IO;
using System.Text;
using System.Web;
using WebMarkupMin.Core;

public class WhitespaceModule : IHttpModule
{
    #region IHttpModule Members

    void IHttpModule.Dispose()
    {
        // Nothing to dispose;
    }

    void IHttpModule.Init(HttpApplication app)
    {
        app.PostRequestHandlerExecute += (o, e) => PostRequestHandlerExecute(app);
    }

    #endregion

    private void PostRequestHandlerExecute(HttpApplication app)
    {
        string contentType = app.Response.ContentType;
        string method = app.Request.HttpMethod;
        int status = app.Response.StatusCode;
        IHttpHandler handler = app.Context.CurrentHandler;

        if (contentType == "text/html" && method == "GET" && status == 200 && handler != null)
        {
            app.Response.Filter = new WhitespaceFilter(app.Response.Filter, app.Request.ContentEncoding);
        }
    }

    #region Stream filter

    private class WhitespaceFilter : Stream
    {
        private readonly Encoding _encoding;
        private readonly Stream _stream;
        private readonly MemoryStream _cache;
        private readonly static HtmlMinifier _minifier = new HtmlMinifier(new HtmlMinificationSettings
        {
            WhitespaceMinificationMode = WhitespaceMinificationMode.Aggressive,
            RemoveRedundantAttributes = false
        });

        public WhitespaceFilter(Stream sink, Encoding encoding)
        {
            _stream = sink;
            _encoding = encoding;
            _cache = new MemoryStream();
        }

        #region Properites

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Length
        {
            get { return 0; }
        }

        private long _position;
        public override long Position
        {
            get { return _position; }
            set { _position = value; }
        }

        #endregion

        #region Methods

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _cache.Write(buffer, offset, count);
        }

        public override void Close()
        {
            byte[] buffer = _cache.ToArray();
            int cacheSize = buffer.Length;

            string original = _encoding.GetString(buffer);
            string result = _minifier.Minify(original).MinifiedContent;
            byte[] output = _encoding.GetBytes(result);

            _stream.Write(output, 0, output.Length);
            _cache.Dispose();
            _stream.Dispose();
        }

        #endregion

    }

    #endregion

}
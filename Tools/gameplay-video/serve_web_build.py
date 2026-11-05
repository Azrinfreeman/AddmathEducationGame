"""Serve an existing Unity Web build locally, including Brotli headers."""

from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path


class UnityBuildHandler(SimpleHTTPRequestHandler):
    def end_headers(self):
        if self.path.endswith('.br'):
            self.send_header('Content-Encoding', 'br')
        super().end_headers()

    def guess_type(self, path):
        if path.endswith('.wasm.br'):
            return 'application/wasm'
        if path.endswith('.js.br'):
            return 'application/javascript'
        if path.endswith('.data.br'):
            return 'application/octet-stream'
        return super().guess_type(path)


if __name__ == '__main__':
    build = Path(__file__).resolve().parents[2] / 'Publish' / 'Web'
    server = ThreadingHTTPServer(('127.0.0.1', 8765), partial(UnityBuildHandler, directory=str(build)))
    print(f'Serving {build} at http://127.0.0.1:8765/', flush=True)
    server.serve_forever()

from flask import Flask, request
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

@app.route('/upload/images', methods=['POST'])
def upload_images():
    # Assuming the images are sent as multipart/form-data
    if 'images' not in request.files:
        return 'No images found', 400

    images = request.files.getlist('images')
    # Process the images here
    # ...

    return 'Images uploaded successfully'

@app.route('/upload/video', methods=['POST'])
def upload_video():
    # Assuming the video is sent as multipart/form-data
    if 'video' not in request.files:
        return 'No video found', 400

    video = request.files['video']
    # Process the video here
    # ...

    return 'Video uploaded successfully'

if __name__ == '__main__':
    app.run()

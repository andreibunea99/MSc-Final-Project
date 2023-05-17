import os
import subprocess
from flask import Flask, request
from flask_cors import CORS
import cv2

app = Flask(__name__)
CORS(app)

UPLOAD_FOLDER = os.path.join(os.getcwd(), '../image_dataset')
VIDEO_FOLDER = os.path.join(os.getcwd(), '../video')
OUTPUT_FOLDER = os.path.join(os.getcwd(), '../output')
app.config['UPLOAD_FOLDER'] = UPLOAD_FOLDER
app.config['VIDEO_FOLDER'] = VIDEO_FOLDER
app.config['OUTPUT_FOLDER'] = OUTPUT_FOLDER

def save_images(file_storage, save_folder):
    os.makedirs(save_folder, exist_ok=True)
    filename = file_storage.filename
    filepath = os.path.join(save_folder, filename)
    file_storage.save(filepath)

    return filepath

def extract_frames(video_path, save_folder, frame_interval=1):
    os.makedirs(save_folder, exist_ok=True)
    save_path_template = os.path.join(save_folder, 'frame{}.jpg')

    video_capture = cv2.VideoCapture(video_path)
    frame_count = int(video_capture.get(cv2.CAP_PROP_FRAME_COUNT))

    for frame_index in range(0, frame_count, frame_interval):
        video_capture.set(cv2.CAP_PROP_POS_FRAMES, frame_index)
        success, frame = video_capture.read()
        if success:
            save_path = save_path_template.format(frame_index)
            cv2.imwrite(save_path, frame)

    video_capture.release()

def run_meshcli_script():
    script_path = os.path.join(os.getcwd(), '..', 'meshroom_CLI', 'Run.bat')
    subprocess.call(script_path, shell=True)

@app.route('/upload/images', methods=['POST'])
def upload_images():
    if 'images' not in request.files:
        return 'No images found', 400

    images = request.files.getlist('images')
    
    # Delete all files from the image dataset folder
    for file_name in os.listdir(app.config['UPLOAD_FOLDER']):
        file_path = os.path.join(app.config['UPLOAD_FOLDER'], file_name)
        if os.path.isfile(file_path):
            os.remove(file_path)

    for image in images:
        save_images(image, app.config['UPLOAD_FOLDER'])

    print('Running meshroom script...')
    run_meshcli_script()

    return 'Images uploaded successfully'

@app.route('/upload/video', methods=['POST'])
def upload_video():
    if 'video' not in request.files:
        return 'No video found', 400

    video = request.files['video']
    video_path = os.path.join(app.config['VIDEO_FOLDER'], 'video.mp4')

    video.save(video_path)

    # Delete all files from the image dataset folder
    for file_name in os.listdir(app.config['UPLOAD_FOLDER']):
        file_path = os.path.join(app.config['UPLOAD_FOLDER'], file_name)
        if os.path.isfile(file_path):
            os.remove(file_path)

    # Extract frames from the video
    extract_frames(video_path, app.config['UPLOAD_FOLDER'], frame_interval=25)

    # Delete the video file
    os.remove(video_path)

    # # Run the meshcli script
    print('Running meshroom script...')
    run_meshcli_script()

    return 'Video uploaded successfully'

if __name__ == '__main__':
    app.run()

## CenterNet on Windows10

### Reference

- [CenterNet](https://github.com/xingyizhou/CenterNet/blob/master/readme/INSTALL.md)

### Install 

- [Optional but recommended] create a new conda environment.

    `conda create --name CenterNet python=3.6`

- And activate the environment.

    `conda activate CenterNet`

- Install pytorch:

    `conda install pytorch=1.4.0 torchvision=0.5.0 -c pytorch`


- Clone source code and Install the requirements:

    ```
    git clone https://github.com/xingyizhou/CenterNet
    cd CenterNet
    pip install -r requirements.txt
    ```

- Install COCOAPI:

    `pip install git+https://github.com/philferriere/cocoapi.git#subdirectory=PythonAPI`

- Compile deformable convolutional:

    - download [DCNv2](https://github.com/CharlesShang/DCNv2/tree/pytorch_1.0) for pytorch1.0  and replace `src\lib\models\networks\DCNv2`

    - modify `src\lib\models\networks\DCNv2\src\cuda\dcn_v2_cuda.cu`

        ```
        //extern THCState *state;                           
        THCState *state = at::globalContext().lazyInitCUDA();   // Modified
        ```
    - python setup 

        `python setup.py build develop`

    - error
        - Cannot find compiler cl.exe in PATH

            - add `C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Tools\MSVC\14.16.27023\bin\Hostx86\x64` to system path

### Test 

- download pretrained model put in `./models`

    - [ctdet_coco_dla_2x](https://drive.google.com/file/d/1pl_-ael8wERdUREEnaIfqOV_VF2bEVRT/view)
    - [multi_pose_dla_3x](https://drive.google.com/file/d/1PO1Ax_GDtjiemEmDVD7oPWwqQkUu28PI/view)

- command

    - `python demo.py ctdet --demo webcam --load_model ../models/ctdet_coco_dla_2x.pth`
    - `python demo.py multi_pose --demo webcam --load_model ../models/multi_pose_dla_3x.pth`

- runtime error

    - `urllib.error.HTTPError: HTTP Error 404: Not Found`
        
        - log:

            ```
            Creating model...
            Downloading: "http://dl.yf.io/dla/models\imagenet\dla34-ba72cf86.pth" to C:\Users\user/.cache\torch\checkpoints\dla34-ba72cf86.pth
            Traceback (most recent call last):
            ...
            urllib.error.HTTPError: HTTP Error 404: Not Found
            ```

        - solve :

            manual download `http://dl.yf.io/dla/models\imagenet\dla34-ba72cf86.pth` and move to `C:\Users\user/.cache\torch\checkpoints\dla34-ba72cf86.pth`

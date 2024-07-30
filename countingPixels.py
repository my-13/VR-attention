import cv2
import numpy as np
import sys

def main(argv=None):
    if argv is None:
        argv = sys.argv
    
    if len(argv) != 2:
        print('Usage: python countingPixels.py filename')
        return
    
    image = cv2.imread(argv[1])

    if (image is None):
        print(f'Error: Could not open or find the image {argv[1]}')
    
    find_blue_objects(image)
    
    return 0

def find_blue_objects(image):
    hsv = cv2.cvtColor(image, cv2.COLOR_BGR2HSV)

    lower_blue = np.array([100,50,50])
    upper_blue = np.array([130,255,255])

    mask = cv2.inRange(hsv, lower_blue, upper_blue)

    # Find clumps of blue pixels
    contours, _ = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

    object_sizes = {}

    # Draw the contours and image
    for i, contour in enumerate(contours):
        area = cv2.contourArea(contour)
        object_sizes[i] = area
        
        cv2.drawContours(image, [contour], -1, (0, 255, 0), 2)
    
    cv2.imshow('Image', image)
    cv2.waitKey(0)
    cv2.destroyAllWindows()

    for i in object_sizes:
        print(f'Object {i} has size {object_sizes[i]}')


if __name__ == "__main__":
    sys.exit(main(sys.argv))
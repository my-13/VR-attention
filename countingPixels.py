import cv2
import numpy as np

image = cv2.imread('pixels.png')

hsv = cv2.cvtColor(image, cv2.COLOR_BGR2HSV)

lower_blue = np.array([100,50,50])
upper_blue = np.array([130,255,255])

mask = cv2.inRange(hsv, lower_blue, upper_blue)

contours, _ = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

object_sizes = {}

for i, contour in enumerate(contours):
    area = cv2.contourArea(contour)
    object_sizes[i] = area
    
    cv2.drawContours(image, [contour], -1, (0, 255, 0), 2)
    
cv2.imshow('Image', image)
cv2.waitKey(0)
cv2.destroyAllWindows()

for i in object_sizes:
    print(f'Object {i} has size {object_sizes[i]}')
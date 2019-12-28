#!/usr/bin/env python3
# Copyright 2019 Amazon.com, Inc. or its affiliates.  All Rights Reserved.
# 
# You may not use this file except in compliance with the terms and conditions 
# set forth in the accompanying LICENSE.TXT file.
#
# THESE MATERIALS ARE PROVIDED ON AN "AS IS" BASIS. AMAZON SPECIFICALLY DISCLAIMS, WITH 
# RESPECT TO THESE MATERIALS, ALL WARRANTIES, EXPRESS, IMPLIED, OR STATUTORY, INCLUDING 
# THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, AND NON-INFRINGEMENT.

import os
import sys
import time
import logging
import json
import random
import threading

from enum import Enum
from agt import AlexaGadget

from ev3dev2.led import Leds
from ev3dev2.sound import Sound
from ev3dev2.motor import OUTPUT_A, OUTPUT_B, OUTPUT_C, MoveTank, SpeedPercent, MediumMotor

# Set the logging level to INFO to see messages from AlexaGadget
logging.basicConfig(level=logging.INFO, stream=sys.stdout, format='%(message)s')
logging.getLogger().addHandler(logging.StreamHandler(sys.stderr))
logger = logging.getLogger(__name__)


class Direction(Enum):
    """
    The list of directional commands and their variations.
    These variations correspond to the skill slot values.
    """
    FORWARD = ['forward', 'forwards', 'go forward']
    BACKWARD = ['back', 'backward', 'backwards', 'go backward', 'zurück']
    LEFT = ['left', 'go left', 'links']
    RIGHT = ['right', 'go right', 'rechts']
    STOP = ['stop', 'brake']


class Command(Enum):
    """
    The list of preset commands and their invocation variation.
    These variations correspond to the skill slot values.
    """
    MOVE_CIRCLE = ['circle', 'spin']
    MOVE_SQUARE = ['square']
    GRAB = ['grab', 'nimm']
    LET_GO = ['loslassen', 'los lassen']


class MindstormsGadget(AlexaGadget):
    """
    A Mindstorms gadget that performs movement based on voice commands.
    Two types of commands are supported, directional movement and preset.
    """

    def __init__(self):
        """
        Performs Alexa Gadget initialization routines and ev3dev resource allocation.
        """
        super().__init__()

        # Ev3dev initialization
        self.leds = Leds()
        self.sound = Sound()
        self.drive = MoveTank(OUTPUT_B, OUTPUT_C)
        self.hand = MediumMotor(OUTPUT_A)

    def on_connected(self, device_addr):
        """
        Gadget connected to the paired Echo device.
        :param device_addr: the address of the device we connected to
        """
        self.leds.set_color("LEFT", "GREEN")
        self.leds.set_color("RIGHT", "GREEN")
        logger.info("{} connected to Echo device".format(self.friendly_name))

    def on_disconnected(self, device_addr):
        """
        Gadget disconnected from the paired Echo device.
        :param device_addr: the address of the device we disconnected from
        """
        self.leds.set_color("LEFT", "BLACK")
        self.leds.set_color("RIGHT", "BLACK")
        logger.info("{} disconnected from Echo device".format(self.friendly_name))

    def on_custom_mindstorms_gadget_control(self, directive):
        """
        Handles the Custom.Mindstorms.Gadget control directive.
        :param directive: the custom directive with the matching namespace and name
        """
        try:
            payload = json.loads(directive.payload.decode("utf-8"))
            print("Control payload: {}".format(payload), file=sys.stderr)

            isList = payload['islist']

            if isList == False:

                control_type = payload["type"]
                        
                if control_type == "move":
                    # Expected params: [direction, duration, speed]
                    self._move(payload["direction"], int(payload["duration"]), int(payload["speed"]))

                if control_type == "command":
                    # Expected params: [command]
                    self._activate(payload["command"])

            else:

                for item in payload['dirs']:         
                
                    count = item["count"]

                    for i in range(count):

                        control_type = item["type"]
                    
                        if control_type == "move":

                            # Expected params: [direction, duration, speed]
                            self._move(item["direction"], int(item["duration"]), int(item["speed"]))

                        if control_type == "command":
                            # Expected params: [command]
                            self._activate(item["command"])


        except KeyError:
            print("Missing expected parameters: {}".format(directive), file=sys.stderr)

    def _move(self, direction, duration: int, speed: int, is_blocking=True):
        """
        Handles move commands from the directive.
        Right and left movement can under or over turn depending on the surface type.
        :param direction: the move direction
        :param duration: the duration in seconds
        :param speed: the speed percentage as an integer
        :param is_blocking: if set, motor run until duration expired before accepting another command
        """
        print("Move command: ({}, {}, {}, {})".format(direction, speed, duration, is_blocking), file=sys.stderr)
        if direction in Direction.FORWARD.value:
            self.drive.on_for_seconds(SpeedPercent(speed), SpeedPercent(speed), duration, block=is_blocking)

        if direction in Direction.BACKWARD.value:
            self.drive.on_for_seconds(SpeedPercent(-speed), SpeedPercent(-speed), duration, block=is_blocking)

        if direction in (Direction.RIGHT.value + Direction.LEFT.value):
            self._turn(direction, speed)
            self.drive.on_for_seconds(SpeedPercent(speed), SpeedPercent(speed), duration, block=is_blocking)


    def _activate(self, command, speed=50):
        """
        Handles preset commands.
        :param command: the preset command
        :param speed: the speed if applicable
        """
        print("Activate command: ({}, {})".format(command, speed), file=sys.stderr)
        if command in Command.MOVE_CIRCLE.value:
            self.drive.on_for_seconds(SpeedPercent(int(speed)), SpeedPercent(5), 12)

        if command in Command.MOVE_SQUARE.value:
            for i in range(4):
                self._move("right", 2, speed, is_blocking=True)

        if command in Command.GRAB.value:
            self.hand.on_for_rotations(SpeedPercent(65), 100)

        if command in Command.LET_GO.value:
            self.hand.on_for_rotations(SpeedPercent(-65), 100)

    def _turn(self, direction, speed):
        """
        Turns based on the specified direction and speed.
        Calibrated for hard smooth surface.
        :param direction: the turn direction
        :param speed: the turn speed
        """
        if direction in Direction.LEFT.value:
            self.drive.on_for_seconds(SpeedPercent(0), SpeedPercent(speed), 2)

        if direction in Direction.RIGHT.value:
            self.drive.on_for_seconds(SpeedPercent(speed), SpeedPercent(0), 2)


if __name__ == '__main__':

    gadget = MindstormsGadget()

    # Set LCD font and turn off blinking LEDs
    os.system('setfont Lat7-Terminus12x6')
    gadget.leds.set_color("LEFT", "BLACK")
    gadget.leds.set_color("RIGHT", "BLACK")

    # Startup sequence
    gadget.sound.play_song((('C4', 'e'), ('D4', 'e'), ('E5', 'q')))
    gadget.leds.set_color("LEFT", "GREEN")
    gadget.leds.set_color("RIGHT", "GREEN")

    # Gadget main entry point
    gadget.main()

    # Shutdown sequence
    gadget.sound.play_song((('E5', 'e'), ('C4', 'e')))
    gadget.leds.set_color("LEFT", "BLACK")
    gadget.leds.set_color("RIGHT", "BLACK")

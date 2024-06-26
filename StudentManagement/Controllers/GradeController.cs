﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystum.Models.DTO;
using StudentManagementSystum.Models;
using StudentManagementSystum.Repository.IRepository;

namespace StudentManagementSystum.Controllers
{
    [Route("api/Grade")]
    [ApiController]
    public class GradeController : ControllerBase
    {
        private readonly IGradeRepository _gradeRepository;
        private readonly IMapper _mapper;
        public GradeController(IGradeRepository gradeRepository,IMapper mapper)
        {
            _gradeRepository = gradeRepository;
            _mapper = mapper;
        }
        [HttpGet]
        public IActionResult GetGrades()
        {
            var grades = _gradeRepository.GetGrades();
            return Ok(grades);
        }

        [HttpGet("{gradeId}", Name = "GetGrade")]
        public IActionResult GetGrade(int gradeId)
        {
            var grade = _gradeRepository.GetGrade(gradeId);
            if (grade == null)
            {
                return NotFound();
            }
            return Ok(grade);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGrade(GradeDTO gradeDTO)
        {
            if (gradeDTO == null)
            {
                return BadRequest("Grade object is null");
            }

            var existingGrade = await _gradeRepository.GetEnrollmentIdAsync(gradeDTO.EnrollmentId);
            if (existingGrade != null)
            {
                ModelState.AddModelError("", $"A grade already exists for the enrollment ID {gradeDTO.EnrollmentId}");
                return Conflict(ModelState);
            }

            var grade = _mapper.Map<Grade>(gradeDTO);
            if (!_gradeRepository.CreateGrade(grade))
            {
                ModelState.AddModelError("", $"Something went wrong saving the grade {grade.GradeValue}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetGrade", new { gradeId = grade.Id }, grade);
        }



        [HttpPut("{gradeId}")]
        public IActionResult UpdateGrade(int gradeId, GradeDTO gradeDTO)
        {
            if (gradeDTO == null || gradeId != gradeDTO.Id)
            {
                return BadRequest("Grade object is null or GradeId does not match");
            }

            var grade = _mapper.Map<Grade>(gradeDTO);
            if (!_gradeRepository.UpdateGrade(grade))
            {
                ModelState.AddModelError("", $"Something went wrong updating the grade {grade.GradeValue}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{gradeId}")]
        public IActionResult DeleteGrade(int gradeId)
        {
            var grade = _gradeRepository.GetGrade(gradeId);
            if (grade == null)
            {
                return NotFound();
            }

            if (!_gradeRepository.DeleteGrade(grade))
            {
                ModelState.AddModelError("", $"Something went wrong deleting the grade {grade.GradeValue}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
        [HttpGet("enrollment/{enrollmentId}")]
        public async Task<IActionResult> GetEnrollmentIdAsync(int enrollmentId)
        {
            try
            {
                var grade = await _gradeRepository.GetEnrollmentIdAsync(enrollmentId);
                if (grade == null)
                {
                    return NotFound();
                }
                return Ok(grade);
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        

    }
}
